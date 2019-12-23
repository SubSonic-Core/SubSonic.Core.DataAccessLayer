using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.MockDbClient.Syntax;
using SubSonic.Infrastructure.Factory;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.Test
{
    public class SubSonicMockDbClientFactory
        : DbProviderFactory<MockDbClientFactory>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Provider Factory Pattern")]
        public static readonly SubSonicMockDbClientFactory Instance = new SubSonicMockDbClientFactory();

        public void ClearBehaviors() => Provider.ClearBehaviors();

        public void AddBehavior(MockCommandBehavior behavior) => Provider.AddBehavior(behavior);
    }
}
