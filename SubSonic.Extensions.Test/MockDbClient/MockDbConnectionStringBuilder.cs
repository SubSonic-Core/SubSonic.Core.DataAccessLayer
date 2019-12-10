using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace SubSonic.Extensions.Test.MockDbClient
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "<Pending>")]
    public class MockDbConnectionStringBuilderDictionary
        : DbConnectionStringBuilder
    {
        public MockDbConnectionStringBuilderDictionary()
        {
        }

        public MockDbConnectionStringBuilderDictionary(bool useOdbcRules) : base(useOdbcRules)
        {
        }
    }
}