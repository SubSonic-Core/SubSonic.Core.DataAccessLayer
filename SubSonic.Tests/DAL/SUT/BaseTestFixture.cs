using NUnit.Framework;
using SubSonic.Extensions.Test.MockDbProvider;
using System.Data.Common;

namespace SubSonic.Tests.DAL.SUT
{
    public class BaseTestFixture
    {
        protected TestDbContext DbContext { get => SetUpSubSonic.DbContext; }

        [SetUp]
        public void SetupTestFixture()
        {
            DbContext.Instance.GetService<DbProviderFactory, MockDbProviderFactory>().ClearBehaviors();
        }
    }
}
