using SubSonic.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
#if X86
    using Factories;
#endif
    public class DbDatabase
    {
        [ThreadStatic]
        private static DbConnection dBSharedConnection;

        private readonly ISubSonicLogger<DbDatabase> logger;
        private readonly DbContext dbContext;
        private readonly DbProviderFactory dbProvider;
        private readonly SqlQueryProvider sqlQueryProvider;

        public DbDatabase(ISubSonicLogger<DbDatabase> logger, DbContext dbContext, DbProviderFactory dbProviderFactory, SqlQueryProvider sqlQueryProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.dbProvider = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
            this.sqlQueryProvider = sqlQueryProvider ?? throw new ArgumentNullException(nameof(sqlQueryProvider));
        }

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

        internal TResult ExecuteQuery<TResult>(Expression expression)
        {
            using (IPerformanceLogger<DbDatabase> performance = logger.Start($"{nameof(ExecuteQuery)}<{typeof(TResult).GetQualifiedTypeName()}>"))
            {
                using (AutomaticConnectionScope scope = new AutomaticConnectionScope(this))
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
