using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace SubSonic.Infrastructure
{
    using Builders;
    using Factory;
    using Linq;
    using Logging;
    using Microsoft.Extensions.Logging;
    using Schema;

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

        public int ExecuteStoredProcedure(DbSubSonicStoredProcedure procedure)
        {
            DbStoredProcedure db = DbStoredProcedureParser.ParseStoredProcedure(procedure);

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, db.Name, db.Parameters))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteStoredProcedure)}"))
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Connection.Open();

                    int result = cmd.ExecuteNonQuery();

                    cmd.Parameters.ApplyOutputParameters(procedure);

                    return result;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        internal IEnumerable<TEntity> ExecuteDbQuery<TEntity>(DbQueryType queryType, IDbEntityModel model, IEnumerable<IEntityProxy> data, out string error)
        {
            IDbQuery query = new DbSqlQueryBuilder(model.EntityModelType, logger)
                                .BuildDbQuery<TEntity>(queryType, data);

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, query.Sql, query.Parameters))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteDbQuery)}"))
            {
                try
                {
                    cmd.Connection.Open();

                    IEnumerable<TEntity> results = cmd.ExecuteReader().Map<TEntity>();

                    error = cmd.Parameters.GetOutputParameter<string>(nameof(error));

                    return results;

                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    error = ex.Message;

                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        public IEnumerable<TEntity> ExecuteStoredProcedure<TEntity>(DbSubSonicStoredProcedure procedure)
        {
            DbStoredProcedure db = DbStoredProcedureParser.ParseStoredProcedure(procedure);

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, db.Name, db.Parameters))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteStoredProcedure)}"))
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Connection.Open();

                    IEnumerable<TEntity> results = cmd.ExecuteReader().Map<TEntity>();

                    cmd.Parameters.ApplyOutputParameters(procedure);

                    return results;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        public bool ExecuteAdapter(IDbQuery query, DataSet data)
        {
            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, query))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteAdapter)}"))
            {
                if (dbContext.Instance.GetService<DbProviderFactory>() is SubSonicDbProvider provider)
                {
                    Scope.Connection.Open();

                    DbDataAdapter adapter = provider.CreateDataAdapter(cmd);

                    return adapter.Fill(data) > 0;
                }

                throw new NotSupportedException();
            }
        }

        public DbDataReader ExecuteReader(string sql, IEnumerable<SubSonicParameter> parameters)
        {
            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, sql, parameters))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteReader)}"))
            {
                return cmd.ExecuteReader();
            }
        }
        
        public DbDataReader ExecuteReader(IDbQuery queryObject)
        {
            if (queryObject is null)
            {
                throw new ArgumentNullException(nameof(queryObject));
            }

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, queryObject))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteReader)}"))
            {
                return cmd.ExecuteReader(queryObject.Behavior);
            }
        }

        public AutomaticConnectionScope GetConnectionScope() => dbContext.Instance.GetService<AutomaticConnectionScope>();

        public static DbCommand GetCommand(IConnectionScope scope, IDbQuery queryObject)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (queryObject is null)
            {
                throw new ArgumentNullException(nameof(queryObject));
            }

            return GetCommand(scope, queryObject.Sql, queryObject.Parameters);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Builder paramatizes all user inputs when sql expression tree is visited")]
        public static DbCommand GetCommand(IConnectionScope scope, string sql, IEnumerable<DbParameter> parameters)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("", nameof(sql));
            }

            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            DbCommand command = scope.Connection.CreateCommand();

            Debug.Assert(command.CommandType == CommandType.Text);

            foreach (DbParameter parameter in parameters)
            {
                DbParameter dbParameter = command.CreateParameter();

                dbParameter.Map(parameter);

                command.Parameters.Add(dbParameter);
            }

            command.CommandText = sql;

            return command;
        }
    }
}
