using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Test.Rigging.MockDbProvider
{
    using Syntax;
    interface IMockCommandExecution
    {
        int ExecuteNonQuery(MockDbCommand cmd);
        object ExecuteScalar(MockDbCommand cmd);
        MockDbDataReaderCollection ExecuteDataReader(MockDbCommand cmd);
    }
}