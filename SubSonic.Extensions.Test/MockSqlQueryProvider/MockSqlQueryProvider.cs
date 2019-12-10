using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.Test
{
    using Infrastructure;
    using MockDbClient;

    class MockSqlQueryProvider
        : SqlQueryProvider
    {
        public static readonly MockSqlQueryProvider Instance = new MockSqlQueryProvider();

        public MockSqlQueryProvider()
            : base(new MockSqlFragment())
        {

        }

        public override string ClientName => typeof(MockDbClientFactory).FullName;
    }
}
