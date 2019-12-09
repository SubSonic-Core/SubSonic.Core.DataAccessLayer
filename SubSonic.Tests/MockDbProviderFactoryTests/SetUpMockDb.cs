using NUnit.Framework;
using SubSonic.Extensions.Test.MockDbProvider;
using System;
using System.Data.Common;

namespace SubSonic.Tests.MockDbProviderFactoryTests
{
    [SetUpFixture]
    public class SetUpMockDb
    {
        public static Type MockDbProviderFactoryType => typeof(MockDbProviderFactory);

        public static string ProviderInvariantName => MockDbProviderFactoryType.Name;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DbProviderFactories.RegisterFactory(ProviderInvariantName, MockDbProviderFactoryType);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DbProviderFactories.UnregisterFactory(ProviderInvariantName);
        }
    }
}