using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using Logging;
    using Schema;

    public partial class DbSqlQueryBuilder
        : DbExpressionAccessor
        , IDbSqlQueryBuilderProvider
    {
        private readonly ISubSonicLogger logger;
        private readonly SubSonicParameterDictionary parameters;

        public DbSqlQueryBuilder(Type dbModelType, ISubSonicLogger logger = null)
            : base()
        {
            if (dbModelType is null)
            {
                throw new ArgumentNullException(nameof(dbModelType));
            }

            this.logger = logger ?? DbContext.ServiceProvider.GetService<ISubSonicLogger>();
            DbEntity = DbContext.DbModel.GetEntityModel(dbModelType.GetQualifiedType());
            DbTable = DbEntity.Table;
            parameters = new SubSonicParameterDictionary();
        }

        #region properties
        public DbSqlQueryType SqlQueryType { get; private set; }

        public IDbEntityModel DbEntity { get; }
        public DbTableExpression DbTable { get; }
        #endregion

        public IDbQuery BuildDbQuery<TEntity>(DbQueryType queryType, IEnumerable<IEntityProxy> proxies)
        {
            IEnumerable<TEntity> entities = proxies.Select(x =>
            {
                if (x is IEntityProxy<TEntity> property)
                {
                    return property.Data;
                }
                return default;
            })
                .Where(x => x.IsNotNull())
                .ToArray();

            switch (queryType)
            {
                case DbQueryType.Insert:
                    return ToQuery(BuildInsertQuery(entities));
                case DbQueryType.Update:
                    return ToQuery(BuildUpdateQuery(entities));
                case DbQueryType.Delete:
                    return ToQuery(BuildDeleteQuery(entities));
                default:
                    throw new NotSupportedException();
            }
        }

        private Expression BuildInsertQuery<TEntity>(IEnumerable<TEntity> entities)
        {
            return DbExpression.DbInsert(DbTable, entities.Select(x => (object)x));
        }

        private Expression BuildUpdateQuery<TEntity>(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        private Expression BuildDeleteQuery<TEntity>(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        protected virtual DbSqlQueryType GetQueryType(Expression expression)
        {
            if (expression.IsNotNull())
            {
                if (!expression.NodeType.IsDbExpression())
                {
                    return DbSqlQueryType.Unknown;
                }

                switch((DbExpressionType)expression.NodeType)
                {
                    case DbExpressionType.Select:
                        return DbSqlQueryType.Read;
                    case DbExpressionType.Insert:
                        return DbSqlQueryType.Create;
                    case DbExpressionType.Update:
                        return DbSqlQueryType.Update;
                    case DbExpressionType.Delete:
                        return DbSqlQueryType.Delete;
                    default:
                        return DbSqlQueryType.Unknown;
                }
            }

            return DbSqlQueryType.Unknown;
        }
    }
}
