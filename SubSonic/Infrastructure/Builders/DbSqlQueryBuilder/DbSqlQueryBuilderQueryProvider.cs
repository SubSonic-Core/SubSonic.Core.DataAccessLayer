using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SubSonic.Infrastructure.Builders
{
    using Logging;
    using Linq.Expressions;
    

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

            if (expression is DbExpression select)
            {
                using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    CmdBehavior = typeof(TResult).IsEnumerable() ? CommandBehavior.Default : CommandBehavior.SingleRow;

                    Type elementType = typeof(TResult).GetQualifiedType();

                    bool isEntityModel = DbContext.DbModel.IsEntityModelRegistered(elementType);

                    if (!isEntityModel ||
                        DbContext.Current.ChangeTracking.Count(elementType, select) == 0)
                    {
                        try
                        {
                            Scope.Connection.Open();

                            DbDataReader reader = Scope.Database.ExecuteReader(ToQuery(select));

                            while (reader.Read())
                            {
                                if (isEntityModel)
                                {
                                    DbContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                                }
                                else
                                {
                                    if (CmdBehavior == CommandBehavior.SingleRow)
                                    {
                                        return Scalar<TResult>(reader);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            Scope.Connection.Close();
                        }
                    }

                    return DbContext.Current.ChangeTracking.Where<TResult>(elementType, this, expression);
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

                    if (method.Method.Name.Equals(nameof(Queryable.Count), StringComparison.CurrentCulture))
                    {   // the method count has been called on the collection
                        return Execute<TResult>(DbExpression.DbSelectAggregate(_select, new[]
                        {
                            DbExpression.DbAggregate(typeof(TResult), AggregateType.Count, _select.Columns.First(x => x.Property.IsPrimaryKey).Expression)
                        }));
                    }
                    else
                    {
                        if (method.Arguments.Count > 1)
                        {
                            return Execute<TResult>(BuildSelect(_select, where));
                        }
                        else
                        {
                            return Execute<TResult>(_select);
                        }
                    }
                }
            }

            throw new NotSupportedException(expression.ToString());
        }

        private TResult Scalar<TResult>(IDataRecord reader)
        {
            if (reader.FieldCount > 1)
            {
                throw new InvalidOperationException();
            }

            return (TResult)Convert.ChangeType(reader[0], typeof(TResult), CultureInfo.CurrentCulture);
        }

        public object Execute(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
            {
                return Scope.Database.ExecuteReader(ToQuery(expression));
            }
        }
    }
}
