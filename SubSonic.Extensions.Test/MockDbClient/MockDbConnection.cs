using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace SubSonic.Extensions.Test.MockDbClient
{
    public class MockDbConnection : DbConnection
    {
        readonly IMockCommandExecution exec;
        ConnectionState state;
        internal MockDbConnection(IMockCommandExecution exec)
        {
            this.exec = exec;
        }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new MockDbTransaction(isolationLevel, this);
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            state = ConnectionState.Closed;
        }

        public override string ConnectionString
        {
            get;
            set;
        }

        protected override DbCommand CreateDbCommand()
        {
            var cmd = new MockDbCommand(exec)
            {
                Connection = this
            };
            return cmd;
        }

        public override string DataSource
        {
            get { return string.Empty; }
        }

        public override string Database
        {
            get { return string.Empty; }
        }

        public override void Open()
        {
            if (state == ConnectionState.Open)
            {
                throw Error.InvalidOperation(MockDBErrors.ConnectionStateAlreadyOpen);
            }

            state = ConnectionState.Open;
        }

        public override string ServerVersion
        {
            get { return string.Empty; }
        }

        public override ConnectionState State
        {
            get { return state; }
        }
    }
}