using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SubSonic.Extensions.Test.MockDbClient
{
    using Syntax;
    using System.Globalization;

    public sealed class MockDbClientFactory : DbProviderFactory, IMockCommandExecution
    {
        private List<MockCommandBehavior> behaviors;

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

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not find behavior for command '{0}'", cmd.CommandText));
        }
        int IMockCommandExecution.ExecuteNonQuery(MockDbCommand cmd)
        {
            return GetReturnValue<int>(cmd);
        }

        object IMockCommandExecution.ExecuteScalar(MockDbCommand cmd)
        {
            return GetReturnValue<object>(cmd);
        }

        MockDbDataReaderCollection IMockCommandExecution.ExecuteDataReader(MockDbCommand cmd)
        {
            return new MockDbDataReaderCollection(GetReturnValue<DataTable>(cmd).CreateDataReader());
        }

        public TReturn GetReturnValue<TReturn>(MockDbCommand cmd)
        {
            if (cmd is null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            object value = FindBehavior(cmd).ReturnValue;

            if (value is Func<DbCommand, TReturn> func)
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
                    if (param_0 is DataTable data)
                    {
                        data.Clear();
                    }

                    return param_0;
                }
            }
            else
            {
                return default(TReturn);
            }

            throw new NotSupportedException();
        }

        public void ClearBehaviors()
        {
            behaviors.Clear();
        }
    }
}