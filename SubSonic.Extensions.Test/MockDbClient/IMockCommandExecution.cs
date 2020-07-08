using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Extensions.Test.MockDbClient
{
    using Syntax;
    
    interface IMockCommandExecution
    {
        int ExecuteNonQuery(MockDbCommand cmd);
        object ExecuteScalar(MockDbCommand cmd);
        MockDbDataReaderCollection ExecuteDataReader(MockDbCommand cmd);
        Task<MockDbDataReaderCollection> ExecuteDataReaderAsync(MockDbCommand cmd, CancellationToken cancellationToken);
    }
}