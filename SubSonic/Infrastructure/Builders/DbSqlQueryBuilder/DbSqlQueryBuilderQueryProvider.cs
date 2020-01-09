using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure.Builders
{
    using Logging;
    using SubSonic.Linq.Expressions;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;

    public partial class DbSqlQueryBuilder
    {
        [DefaultValue(CommandBehavior.Default)]
        protected CommandBehavior CmdBehavior { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                return new SubSonicCollection(DbEntity.EntityModelType, this, BuildQuery(expression));
            }
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return new SubSonicCollection<TEntity>(this, BuildQuery(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression is DbSelectExpression select)
            {
                using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    CmdBehavior = typeof(TResult).IsEnumerable() ? CommandBehavior.Default : CommandBehavior.SingleRow;

                    Type elementType = typeof(TResult).GetQualifiedType();

                    if (DbContext.ChangeTracking.Count(elementType, select) == 0)
                    {
                        try
                        {
                            Scope.Connection.Open();

                            DbDataReader reader = Scope.Database.ExecuteReader(ToQueryObject(select));

                            while (reader.Read())
                            {
                                DbContext.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                            }
                        }
                        finally
                        {
                            Scope.Connection.Close();
                        }
                    }

                    return DbContext.ChangeTracking.Where<TResult>(elementType, this, expression);
                }
            }
            else if (expression is MethodCallExpression method)
            {   // Linq call is coming from System.Linq namespace directly.
                // expression needs to be rebuilt into something the DAL can use
                while (method.Arguments[0] is MethodCallExpression _method)
                {
                    method = _method;
                }

                if (method.Arguments[0] is DbSelectExpression _select)
                {
                    Expression where = null;

                    for(int i = 1, n = method.Arguments.Count; i < n; i++)
                    {
                        where = BuildWhere(_select.From, where, _select.Type, method.Arguments[i]);
                    }

                    return Execute<TResult>(BuildSelect(_select, where));
                }
            }

            throw new NotSupportedException(expression.ToString());
        }

        public object Execute(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
            {
                return Scope.Database.ExecuteReader(ToQueryObject(expression));
            }
        }
    }
}
