using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return Task.Run(() => ExecuteDbDataReader(behavior), cancellationToken);
        }

        public override int ExecuteNonQuery()
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

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => ExecuteNonQuery(), cancellationToken);
        }

        public override object ExecuteScalar()
        {
            Prepare();

            return _exec.ExecuteScalar(this);
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
            if (CommandType == CommandType.Text)
            {
                if (ParameterRegex.IsMatch(this.CommandText))
                {
                    foreach (Match match in ParameterRegex.Matches(this.CommandText))
                    {
                        if (!(Parameters[match.Value] is null))
                        {
                            object value = Parameters[match.Value].Value;
#if NETSTANDARD2_0
                            if (!CommandText.Contains("EXEC") && !(value is DataTable))
#elif NETSTANDARD2_1
                            if (!CommandText.Contains("EXEC", StringComparison.CurrentCulture) && !(value is DataTable))
#endif
                            {
#if NETSTANDARD2_0
                                CommandText = CommandText.Replace(match.Value, (value is string || value is Guid) ? $"'{value}'" : value.ToString());
#elif NETSTANDARD2_1
                                CommandText = CommandText.Replace(match.Value, (value is string || value is Guid) ? $"'{value}'" : value.ToString(), StringComparison.CurrentCulture);
#endif
                            }
                        }
                    }
                }
            }
            else if (CommandType == CommandType.StoredProcedure)
            {

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