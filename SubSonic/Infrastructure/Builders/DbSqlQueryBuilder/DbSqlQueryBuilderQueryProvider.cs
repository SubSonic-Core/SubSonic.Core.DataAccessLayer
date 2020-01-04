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
            using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
            {
                CmdBehavior = typeof(TResult).IsEnumerable() ? CommandBehavior.Default : CommandBehavior.SingleRow;

                Type elementType = typeof(TResult).GetQualifiedType();

                if (DbContext.Cache.Count(elementType, expression) == 0)
                {
                    try
                    {
                        Scope.Connection.Open();

                        DbDataReader reader = Scope.Database.ExecuteReader(ToQueryObject(expression));
                        
                        while (reader.Read())
                        {
                            DbContext.Cache.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                        }
                    }
                    finally
                    {
                        Scope.Connection.Close();
                    }
                }

                return DbContext.Cache.Where<TResult>(elementType, this, expression);
            }
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
