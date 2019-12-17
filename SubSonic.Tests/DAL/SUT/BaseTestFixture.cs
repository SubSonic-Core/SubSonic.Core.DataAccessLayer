using NUnit.Framework;
using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.Models;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace SubSonic.Tests.DAL.SUT
{
    public class BaseTestFixture
    {
        protected TestDbContext DbContext { get => SetUpSubSonic.DbContext; }

        [SetUp]
        public virtual void SetupTestFixture()
        {
            DbContext.Instance.GetService<DbProviderFactory, MockDbClientFactory>().ClearBehaviors();
        }

        protected IEnumerable<Status> Statuses { get; set; }
        protected IEnumerable<Unit> Units { get; set; }
        protected IEnumerable<RealEstateProperty> RealEstateProperties { get; set; }
    }
}
