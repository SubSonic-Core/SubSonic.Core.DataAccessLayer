using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SubSonic.Extensions.Test.MockDbClient
{
    public class MockDbTransaction
        : DbTransaction
    {
        private readonly IsolationLevel _isolationLevel;

        public override IsolationLevel IsolationLevel { get { return _isolationLevel; } }

        private readonly DbConnection _dbConnection;

        protected override DbConnection DbConnection { get { return _dbConnection; } }

        public override void Commit()
        {   // in a mock setting I am not sure how this functionality is mockable

        }

        public override void Rollback()
        {   // in a mock setting I am not sure how this functionality is mockable

        }

        internal MockDbTransaction(IsolationLevel isolationLevel, DbConnection connection)
        {
            _isolationLevel = isolationLevel;
            _dbConnection = connection;
        }
    }
}