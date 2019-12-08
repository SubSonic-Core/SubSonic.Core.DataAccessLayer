using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace SubSonic.Test.Rigging.MockDbProvider
{
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