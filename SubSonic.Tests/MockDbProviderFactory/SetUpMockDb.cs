using NUnit.Framework;
using SubSonic.Extensions.Test.MockDbClient;
using System;
using System.Data.Common;

namespace SubSonic.Tests.MockDbProviderFactory
{
    [SetUpFixture]
    public class SetUpMockDb
    {
        public static Type MockDbProviderFactoryType => typeof(MockDbClientFactory);

        public static string ProviderInvariantName => MockDbProviderFactoryType.Name;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
#if NETFRAMEWORK
            DbProviderFactories.RegisterFactory(ProviderInvariantName, MockDbProviderFactoryType);
#else
            DbProviderFactories.RegisterFactory(ProviderInvariantName, MockDbProviderFactoryType);
#endif
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if NETFRAMEWORK
            DbProviderFactories.UnregisterFactory(ProviderInvariantName);
#else
            DbProviderFactories.UnregisterFactory(ProviderInvariantName);
#endif
        }
    }
}