﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SubSonic.Test.Rigging.MockDbProvider
{
    using Syntax;
    using System.Globalization;

    public class MockDbProviderFactory : DbProviderFactory, IMockCommandExecution
    {
        private List<MockCommandBehavior> behaviors;

        public MockDbProviderFactory()
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
                if (behavior.Matches(cmd))
                    return behavior;
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not find behavior for command '{0}'", cmd.CommandText));
        }
        int IMockCommandExecution.ExecuteNonQuery(MockDbCommand cmd)
        {
            return (int)FindBehavior(cmd).ReturnValue;
        }

        object IMockCommandExecution.ExecuteScalar(MockDbCommand cmd)
        {
            return FindBehavior(cmd).ReturnValue;
        }

        MockDbDataReaderCollection IMockCommandExecution.ExecuteDataReader(MockDbCommand cmd)
        {
            return new MockDbDataReaderCollection(((DataTable)FindBehavior(cmd).ReturnValue).CreateDataReader());
        }

        public void ClearBehaviors()
        {
            behaviors.Clear();
        }
    }
}