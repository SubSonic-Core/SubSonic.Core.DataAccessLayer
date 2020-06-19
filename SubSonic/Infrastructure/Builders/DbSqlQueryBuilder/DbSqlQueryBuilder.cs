using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using Logging;
    using Schema;    

    public partial class DbSqlQueryBuilder
        : DbExpressionAccessor
        , ISubSonicQueryProvider
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

            this.logger = logger ?? DbContext.ServiceProvider.GetService<ISubSonicLogger<DbSqlQueryBuilder>>();

            if (DbContext.DbModel.TryGetEntityModel(dbModelType.GetQualifiedType(), out IDbEntityModel model))
            {
                DbEntity = model;
                DbTable = DbEntity.Table;
            }

            parameters = new SubSonicParameterDictionary();
        }

        #region properties
        public DbSqlQueryType SqlQueryType { get; private set; }
        public IDbEntityModel DbEntity { get; }
        public DbTableExpression DbTable { get; private set; }
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
                    // delete queries can not be alaised.
                    DbTable = (DbTableExpression)DbExpression.DbTable(DbEntity, null);

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
            Expression
                logical = null;

            LambdaExpression
                predicate = null;

            foreach (string key in DbEntity.GetPrimaryKey())
            {
                PropertyInfo property = DbEntity.EntityModelType.GetProperty(key);
                
                if (DbTable.Model.DefinedTableTypeExists)
                {
                    ISubSonicCollection<TEntity> queryable = new SubSonicTableTypeCollection<TEntity>("input");

                    queryable.AddRange(entities);

                    logical = BuildLogicalIn(
                        logical,
                        property,
                        queryable.Select(DbEntity[key]),
                        DbGroupOperator.AndAlso);
                }
                else
                {
                    IEnumerable<Expression> values = entities
                        .Select(entity =>
                            Expression.Constant(property.GetValue(entity)));

                    logical = BuildLogicalIn(
                        logical,
                        property,
                        values,
                        DbGroupOperator.AndAlso);
                }
            }

            predicate = (LambdaExpression)BuildLambda(logical, LambdaType.Predicate);

            return DbExpression.DbDelete(
                    entities,
                    DbTable,
                    DbExpression.Where(DbTable, DbEntity.EntityModelType, predicate));
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
