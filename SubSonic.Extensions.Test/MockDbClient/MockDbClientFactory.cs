using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Extensions.Test.MockDbClient
{
    using Syntax;
    using System.Globalization;

    public sealed class MockDbClientFactory : DbProviderFactory, IMockCommandExecution
    {
        private readonly List<MockCommandBehavior> behaviors;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "ProviderFactory Pattern")]
        public static readonly MockDbClientFactory Instance = new MockDbClientFactory();

        public MockDbClientFactory()
            : base()
        {
            behaviors = new List<MockCommandBehavior>();
        }
        public void AddBehavior(MockCommandBehavior behavior)
        {
            behaviors.Add(behavior);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "used for unit testing to verify the command was sent")]
        public int RecievedBehavior(string command, CommandType type = CommandType.Text)
        {
            using (var cmd = new MockDbCommand(command, type))
            {
                int count = FindBehavior(cmd).IsNotNull(behavior => behavior.GetRecievedCount());

                return count;
            }
        }

        public override DbConnection CreateConnection()
        {
            return new MockDbConnection(this);
        }
        public override DbCommand CreateCommand()
        {
            return new MockDbCommand(this);
        }
        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new MockDbCommandBuilder();
        }
        public override DbDataAdapter CreateDataAdapter()
        {
            return new MockDbDataAdapter(this);
        }
        public override DbParameter CreateParameter()
        {
            return new MockDbParameter();
        }
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new MockDbConnectionStringBuilderDictionary();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return base.CreateDataSourceEnumerator();
        }

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return false;
            }
        }

        private MockCommandBehavior FindBehavior(MockDbCommand cmd)
        {
            foreach (var behavior in behaviors)
            {
                if (behavior.Matches(cmd))
                {
                    return behavior;
                }
            }

            return null;
        }

        int IMockCommandExecution.ExecuteNonQuery(MockDbCommand cmd)
        {
            if (GetReturnValue<int>(cmd) is int @return)
            {
                return @return;
            }

            throw Error.InvalidOperation();
        }

        object IMockCommandExecution.ExecuteScalar(MockDbCommand cmd)
        {
            return GetReturnValue<object>(cmd);
        }

        async Task<MockDbDataReaderCollection> IMockCommandExecution.ExecuteDataReaderAsync(MockDbCommand cmd, CancellationToken cancellationToken)
        {
            string[] commands = cmd.CommandText.Split(';');

            if (commands.Length == 1)
            {   // command contains one select command
                object value = GetReturnValue<DataTable>(cmd);

                if (value is DataTable result)
                {
                    return new MockDbDataReaderCollection(result.CreateDataReader());
                }
                else if (value is null)
                {
                    return new MockDbDataReaderCollection();
                }
                else
                {
                    throw Error.InvalidOperation(MockDBErrors.ResultSetIsNotDataTable);
                }
            }
            else
            {
                using (DataSet data = new DataSet())
                {
                    foreach (string sql in commands)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
#pragma warning disable CA2000 // Dispose objects before losing scope
                        if (GetReturnValue<DataTable>(new MockDbCommand(this, cmd.Parameters) { CommandText = sql.Trim("\r\n".ToCharArray()) }) is DataTable result)
                        {
                            data.Tables.Add(result);
                        }
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    }

                    if (data.Tables.Count == 0)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not find behavior for command '{0}'", cmd.CommandText));
                    }

                    return new MockDbDataReaderCollection(data.CreateDataReader());
                }
            }
        }

        MockDbDataReaderCollection IMockCommandExecution.ExecuteDataReader(MockDbCommand cmd)
        {
            string[] commands = cmd.CommandText.Split(';');

            if (commands.Length == 1)
            {   // command contains one select command
                object value = GetReturnValue<DataTable>(cmd);

                if (value is DataTable result)
                {
                    return new MockDbDataReaderCollection(result.CreateDataReader());
                }
                else if (value is null)
                {
                    return new MockDbDataReaderCollection();
                }
                else
                {
                    throw Error.InvalidOperation(MockDBErrors.ResultSetIsNotDataTable);
                }
            }
            else
            {
                using (DataSet data = new DataSet())
                {
                    foreach (string sql in commands)
                    {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
#pragma warning disable CA2000 // Dispose objects before losing scope
                        if (GetReturnValue<DataTable>(new MockDbCommand(this, cmd.Parameters) { CommandText = sql.Trim("\r\n".ToCharArray()) }) is DataTable result)
                        {
                            data.Tables.Add(result);
                        }
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    }

                    if (data.Tables.Count == 0)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not find behavior for command '{0}'", cmd.CommandText));
                    }

                    return new MockDbDataReaderCollection(data.CreateDataReader());
                }
            }
        }

        public object GetReturnValue<TReturn>(MockDbCommand cmd)
        {
            if (cmd is null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            MockCommandBehavior behavior = FindBehavior(cmd);

            if (behavior is null)
            {
                return default(TReturn);
            }

            object value = behavior.ReturnValue;

            if (value is Func<DbCommand, object> func)
            {
                return func(cmd);
            }
            else if (value is TReturn @return)
            {
                return @return;
            }
            else if (cmd.Parameters.Count > 0)
            {   
                if (cmd.Parameters[0].Value is TReturn param_0)
                {
                    if ((object)param_0 is DataTable data)
                    {
                        data.Clear();
                    }

                    return param_0;
                }
            }
            else
            {
#pragma warning disable IDE0034 // Simplify 'default' expression
                return default(TReturn);
#pragma warning restore IDE0034 // Simplify 'default' expression
            }

            throw new NotSupportedException();
        }

        public void ClearBehaviors()
        {
            behaviors.Clear();
        }
    }
}