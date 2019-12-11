using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Builders
{
    using Schema;
    using Logging;

    public class DbSqlQueryBuilder<TEntity>
        : IDbSqlQueryBuilder
    {
        private readonly DbContext dbContext;
        private readonly ISubSonicLogger<DbSqlQueryBuilder<TEntity>> logger;
        private readonly IDbEntityModel dbEntityModel;

        public DbSqlQueryBuilder(DbContext dbContext, ISubSonicLogger<DbSqlQueryBuilder<TEntity>> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbEntityModel = this.dbContext.Model.GetEntityModel(typeof(TEntity).GetQualifiedType());
        }

        public SqlQueryType SqlQueryType { get; private set; }

        public Expression Expression { get; private set; }

        public ISqlQueryProvider SqlQueryProvider { get; private set; }

        private void CheckBuilderState()
        {
            if (SqlQueryProvider.IsNull())
            {
                throw new InvalidOperationException();
            }
        }

        public IDbSqlQueryBuilder BuildSqlQuery(SqlQueryType sqlQueryType, ISqlQueryProvider sqlQueryProvider)
        {
            SqlQueryType = sqlQueryType;
            SqlQueryProvider = sqlQueryProvider ?? throw new ArgumentNullException(nameof(sqlQueryProvider));

            return this;
        }

        public IDbSqlQueryBuilder BuildSqlQuery(Expression expression)
        {
            using (var _perf = logger.Start("BuildSqlQuery"))
            {
                CheckBuilderState();

                Expression = expression ?? throw new ArgumentNullException(nameof(expression));

                SqlQueryProvider.EntityModel = dbEntityModel;

                switch(SqlQueryType)
                {
                    case SqlQueryType.Create:
                        BuildCreateQuery();
                        break;
                    case SqlQueryType.Read:
                        BuildReadQuery();
                        break;
                    case SqlQueryType.Update:
                        BuildUpdateQuery();
                        break;
                    case SqlQueryType.Delete:
                        BuildDeleteQuery();
                        break;
                }

                return this;
            }
        }

        public IDbSqlQueryBuilder BuildCreateQuery()
        {
            throw new NotImplementedException();
        }

        public IDbSqlQueryBuilder BuildReadQuery()
        {
            SqlQueryProvider.BuildSelectStatement();

            return this;
        }

        public IDbSqlQueryBuilder BuildUpdateQuery()
        {
            throw new NotImplementedException();
        }

        public IDbSqlQueryBuilder BuildDeleteQuery()
        {
            throw new NotImplementedException();
        }

        public object ToQueryObject()
        {
            return new
            {
                sql = (string)null,
                parameters = new object[] { new { } }
            };
        }

        
    }
}
