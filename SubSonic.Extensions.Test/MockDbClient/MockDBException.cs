using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Extensions.Test.MockDbClient
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public sealed class MockDBException
        : DbException
    {
        public MockDBException(string message) 
            : base(message)
        {
        }

        public MockDBException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
