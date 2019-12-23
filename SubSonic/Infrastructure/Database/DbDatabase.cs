using SubSonic.Infrastructure.Logging;
using System;
using System.Data.Common;

namespace SubSonic.Infrastructure
{
    using Linq;
#if DB_PROVIDER_NOT_DEFINED
    using Factories;
#endif
    public class DbDatabase
        : IInfrastructure<DbProviderFactory>
    {
        [ThreadStatic]
        private static DbConnection dBSharedConnection;

        private readonly ISubSonicLogger<DbDatabase> logger;
        private readonly DbContext dbContext;
        private readonly DbProviderFactory dbProvider;
        private readonly ISqlQueryProvider queryProvider;

        public DbDatabase(ISubSonicLogger<DbDatabase> logger, DbContext dbContext, DbProviderFactory dbProvider, ISqlQueryProvider queryProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
            this.queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        public DbProviderFactory Instance => dbProvider;

        #region connections
        internal DbConnection CurrentSharedConnection
        {
            get
            {
                return dBSharedConnection;
            }
            set
            {
                if (value.IsNull())
                {
                    dBSharedConnection.IsNotNull(Con => Con.Dispose());
                    dBSharedConnection = null;
                }
                else
                {
                    dBSharedConnection = value;
                    dBSharedConnection.Disposed += DBSharedConnection_Disposed;
                }
            }
        }

        private static void DBSharedConnection_Disposed(object sender, EventArgs e)
        {
            dBSharedConnection = null;
        }

        internal DbConnection InitializeSharedConnection()
        {
            if (CurrentSharedConnection == null)
                CurrentSharedConnection = CreateConnection();

            return CurrentSharedConnection;
        }

        internal DbConnection CreateConnection()
        {
            DbConnection connection = dbProvider.CreateConnection();
            if (dbContext.GetConnectionString.IsNotNull())
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();

                connection.ConnectionString = dbContext.GetConnectionString(builder, dbContext.Options);
            }
            return connection;
        }

        internal void ResetSharedConnection()
        {
            CurrentSharedConnection.IsNotNull(Con => Con.Dispose());
        }
        #endregion
    }
}
