using Microsoft.Extensions.Logging;
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
    using Schema;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

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
                    dBSharedConnection?.Dispose();
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
            CurrentSharedConnection?.Dispose();
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

                    db.CleanUpParameters();

                    return result;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        internal ISubSonicQueryProvider GetQueryBuilderFor(IDbEntityModel model)
        {
            return new DbSqlQueryBuilder(model.EntityModelType, logger);
        }

        internal IEnumerable<TEntity> ExecuteDbQuery<TEntity>(ISubSonicQueryProvider provider, DbQueryType queryType, IEnumerable<IEntityProxy> data, out string error)
        {
            IDbQuery query = provider.BuildDbQuery<TEntity>(queryType, data);

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, query.Sql, query.Parameters))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteDbQuery)}"))
            {
                try
                {
                    logger.LogTrace(query.Sql);

                    cmd.Connection.Open();

                    IEnumerable<TEntity> results = Array.Empty<TEntity>();

                    switch (queryType)
                    {
                        case DbQueryType.Delete:
                            int count = cmd.ExecuteNonQuery();

                            logger.LogTrace($"{count} records affected.");
                            break;
                        case DbQueryType.Insert:
                        case DbQueryType.Update:
                            results = cmd.ExecuteReader().ReadData<TEntity>();
                            break;
                    }

                    error = null;

                    foreach (DbParameter parameter in cmd.Parameters)
                    {
                        if (parameter.Direction == ParameterDirection.Input)
                        {
                            if (parameter.Value is DataTable table)
                            {
                                table.Dispose();
                                parameter.Value = null;
                            }

                            continue;
                        }

                        if (parameter.ParameterName.Equals($"@{nameof(error)}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            error = cmd.Parameters.GetOutputParameter<string>(nameof(error));
                        }                        
                    }

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

                    IEnumerable<TEntity> results = Array.Empty<TEntity>();

                    if (!db.IsNonQuery)
                    {
                        results = cmd.ExecuteReader().ReadData<TEntity>();
                    }
                    else
                    {
                        int cnt = cmd.ExecuteNonQuery();

                        logger.LogInformation($"{cnt} record(s) affected.");
                    }

                    cmd.Parameters.ApplyOutputParameters(procedure);

                    db.CleanUpParameters();

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

        public async Task<DbDataReader> ExecuteReaderAsync(IDbQuery dbQuery, CancellationToken cancellationToken)
        {
            if (dbQuery is null)
            {
                throw Error.ArgumentNull(nameof(dbQuery));
            }

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, dbQuery))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteReaderAsync)}"))
            {
                logger.LogTrace(dbQuery.Sql);

                try
                {
                    return await cmd
                        .ExecuteReaderAsync(dbQuery.Behavior, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (DataException ex)
                {
                    logger.LogCritical(ex, ex.Message);

                    throw;
                }
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
                logger.LogTrace(queryObject.Sql);

                try
                {
                    return cmd.ExecuteReader(queryObject.Behavior);
                }
                catch(DataException ex)
                { 
                    logger.LogCritical(ex, ex.Message);

                    throw;
                }
            }
        }

        public object ExecuteScalar(IDbQuery dbQuery)
        {
            if (dbQuery is null)
            {
                throw Error.ArgumentNull(nameof(dbQuery));
            }

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, dbQuery))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteScalar)}"))
            {
                logger.LogTrace(dbQuery.Sql);

                try
                {
                    return cmd.ExecuteScalar();
                }
                catch (DataException ex)
                {
                    logger.LogCritical(ex, ex.Message);

                    throw;
                }
            }
        }

        public async Task<object> ExecuteScalarAsync(IDbQuery dbQuery, CancellationToken cancellationToken)
        {
            if (dbQuery is null)
            {
                throw Error.ArgumentNull(nameof(dbQuery));
            }

            using (AutomaticConnectionScope Scope = GetConnectionScope())
            using (DbCommand cmd = GetCommand(Scope, dbQuery))
            using (var perf = logger.Start(GetType(), $"{nameof(ExecuteScalarAsync)}"))
            {
                logger.LogTrace(dbQuery.Sql);

                try
                {
                    return await cmd
                        .ExecuteScalarAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (DataException ex)
                {
                    logger.LogCritical(ex, ex.Message);

                    throw;
                }
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
