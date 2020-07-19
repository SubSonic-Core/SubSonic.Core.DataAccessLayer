using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SubSonic.Logging;

namespace SubSonic.Builders
{
    using Collections;
    using Linq;
    using Linq.Expressions;
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

            this.logger = logger ?? SubSonicContext.ServiceProvider.GetService<ISubSonicLogger<DbSqlQueryBuilder>>();

            if (SubSonicContext.DbModel.TryGetEntityModel(dbModelType.GetQualifiedType(), out IDbEntityModel model))
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
                if (x is IEntityProxy<TEntity> entity)
                {
                    entity.EnsureForeignKeys();

                    return entity.Data;
                }
                return default;
            })
                .Where(x => x.IsNotNull())
                .ToArray();

            Expression expression = null;

            switch (queryType)
            {
                case DbQueryType.Insert:
                    expression = BuildInsertQuery(entities);
                    break;
                case DbQueryType.Update:
                    expression = BuildUpdateQuery(entities);
                    break;
                case DbQueryType.Delete:
                    // delete queries can not be alaised.
                    DbTable = (DbTableExpression)DbExpression.DbTable(DbEntity, null);

                    expression = BuildDeleteQuery(entities);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return ToQuery(expression);
        }

        private Expression BuildInsertQuery<TEntity>(IEnumerable<TEntity> entities)
        {
            return DbExpression.DbInsert(DbTable, entities.Select(x => Expression.Constant(x)));
        }

        private Expression BuildUpdateQuery<TEntity>(IEnumerable<TEntity> entities)
        {
            if (DbEntity.DefinedTableTypeExists)
            {
                return DbExpression.DbUpdate(DbTable, entities.Select(x => Expression.Constant(x)), "update");
            }
            else
            {
                ConstantExpression entity = Expression.Constant(entities.Single());

                if (entity.Value is IEntityProxy proxy)
                {
                    return DbExpression.DbUpdate(DbTable,
                        BuildWhereFindByIDPredicate(
                            DbTable,
                            proxy.KeyData.ToArray(),
                            DbEntity.GetPrimaryKey().ToArray()),
                        new[] { entity });
                }

                throw new NotSupportedException();
            }
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

            MethodInfo method = typeof(Queryable).GetGenericMethod(nameof(Queryable.Where),
                new[] { DbTable.Type.GenericTypeArguments[0] }, 
                DbTable.Type,
                predicate.GetType());

            return DbExpression.DbDelete(
                    entities,
                    DbTable,
                    DbWherePredicateBuilder.GetWhereTranslation(
                        DbExpression.DbWhere(method, new Expression[] { DbTable, predicate })));
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
