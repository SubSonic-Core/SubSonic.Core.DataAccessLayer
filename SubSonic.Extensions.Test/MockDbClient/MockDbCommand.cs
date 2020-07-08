using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace SubSonic.Extensions.Test.MockDbClient
{
    public class MockDbCommand : DbCommand
    {
        readonly IMockCommandExecution _exec;
        readonly MockDbParameterCollection parameters;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "this is an internal call for unit test purpose to ensure command was recieved.")]
        internal MockDbCommand(string command, CommandType type = CommandType.Text)
        {
            _exec = null;
            parameters = new MockDbParameterCollection();
            CommandText = command;
            CommandType = type;
        }

        internal MockDbCommand(IMockCommandExecution exec)
        {
            _exec = exec;
            parameters = new MockDbParameterCollection();
            CommandType = CommandType.Text;
        }

        internal MockDbCommand(IMockCommandExecution exec, DbParameterCollection collection)
            : this(exec)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection is MockDbParameterCollection parameters)
            {
                this.parameters = parameters;
            }
        }

        protected static Regex ParameterRegex => new Regex(@"\@([^=<>\s]+)(?:[a-z]|[0-9]|_|\b)", RegexOptions.Multiline | RegexOptions.Compiled);

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override string CommandText
        {
            get;
            set;
        }

        public override int CommandTimeout
        {
            get;
            set;
        }

        public override CommandType CommandType
        {
            get;
            set;
        }

        protected override DbParameter CreateDbParameter()
        {
            return new MockDbParameter();
        }

        protected override DbConnection DbConnection
        {
            get;
            set;
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get;
            set;
        }

        public override bool DesignTimeVisible
        {
            get;
            set;
        }

        protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        {
            try
            {
                if (this.Connection.State == ConnectionState.Open)
                {
                    Prepare();

                    return _exec.ExecuteDataReader(this);
                }
                else
                {
                    throw new InvalidOperationException(MockDBErrors.ConnectionStateNotOpen);
                }
            }
            catch(Exception ex)
            {
                throw new MockDBException(ex.Message, ex);
            }
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            try
            {
                if (this.Connection.State == ConnectionState.Open)
                {
                    Prepare();

                    return await _exec.ExecuteDataReaderAsync(this, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException(MockDBErrors.ConnectionStateNotOpen);
                }
            }
            catch (Exception ex)
            {
                throw new MockDBException(ex.Message, ex);
            }
        }

        public override int ExecuteNonQuery()
        {
            try
            {
                if (this.Connection.State == ConnectionState.Open)
                {
                    Prepare();

                    return _exec.ExecuteNonQuery(this);
                }
                else
                {
                    throw new InvalidOperationException(MockDBErrors.ConnectionStateNotOpen);
                }
            }
            catch (Exception ex)
            {
                throw new MockDBException(ex.Message, ex);
            }
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => ExecuteNonQuery(), cancellationToken);
        }

        public override object ExecuteScalar()
        {
            try
            {
                Prepare();

                return _exec.ExecuteScalar(this);
            }
            catch (Exception ex)
            {
                throw new MockDBException(ex.Message, ex);
            }
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => ExecuteScalar(), cancellationToken);
        }


        /// <summary>
        /// Prepare the command for execution against the data source
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "prepare is looking for parameters and replacing with real values")]
        public override void Prepare()
        {
            if (CommandType != CommandType.TableDirect)
            {
                if (ParameterRegex.IsMatch(this.CommandText))
                {
                    foreach (Match match in ParameterRegex.Matches(this.CommandText))
                    {
                        // enhance the regex compare to ignore parameter if preceded by a declare
                        if (!match.Value.Equals("@output", StringComparison.CurrentCulture) && 
                            Parameters[match.Value] is null)
                        {
                            throw new ArgumentException(MockDBErrors.MissingDbParameter, match.Value);
                        }
                    }
                }
            }
            else if (CommandType == CommandType.TableDirect)
            {

            }
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get;
            set;
        }
    }
}