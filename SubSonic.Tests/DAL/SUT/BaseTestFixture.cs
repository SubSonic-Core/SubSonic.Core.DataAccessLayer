using NUnit.Framework;
using SubSonic.Extensions.Test.MockDbClient;
using System.Data.Common;

namespace SubSonic.Tests.DAL.SUT
{
    public class BaseTestFixture
    {
        protected TestDbContext DbContext { get => SetUpSubSonic.DbContext; }

        [SetUp]
        public void SetupTestFixture()
        {
            DbContext.Instance.GetService<DbProviderFactory, MockDbClientFactory>().ClearBehaviors();
        }
    }
}
