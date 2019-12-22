using System;
using System.Linq.Expressions;

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
                    case DbExpressionType.Table:
                        return DbSqlQueryType.Unknown;
                    case DbExpressionType.Select:
                        return DbSqlQueryType.Read;
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new ArgumentNullException(nameof(expression));
        }
        
        
    }
}
