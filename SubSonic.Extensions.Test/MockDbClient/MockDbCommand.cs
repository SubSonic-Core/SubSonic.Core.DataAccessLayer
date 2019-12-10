using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace SubSonic.Extensions.Test.MockDbClient
{
    public class MockDbCommand : DbCommand
    {
        IMockCommandExecution _exec;
        MockDbParameterCollection parameters;
        internal MockDbCommand(IMockCommandExecution exec)
        {
            _exec = exec;
            parameters = new MockDbParameterCollection();
        }
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

        public override System.Data.CommandType CommandType
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
            return _exec.ExecuteDataReader(this);
        }

        public override int ExecuteNonQuery()
        {
            return _exec.ExecuteNonQuery(this);
        }

        public override object ExecuteScalar()
        {
            return _exec.ExecuteScalar(this);
        }

        public override void Prepare()
        {
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get;
            set;
        }
    }
}