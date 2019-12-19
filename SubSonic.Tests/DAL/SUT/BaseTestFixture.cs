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

            Statuses = new List<Status>()
            {
                new Status() { ID = 1, Name = "Vacant", IsAvailableStatus = true },
                new Status() { ID = 2, Name = "Renovation", IsAvailableStatus = true },
                new Status() { ID = 3, Name = "Occupied", IsAvailableStatus = false },
            };

            Units = new List<Unit>()
            {
                new Unit() { ID = 1, RealEstatePropertyID = 1, StatusID = 1, NumberOfBedrooms = 1 },
                new Unit() { ID = 2, RealEstatePropertyID = 1, StatusID = 2, NumberOfBedrooms = 2 },
                new Unit() { ID = 3, RealEstatePropertyID = 1, StatusID = 3, NumberOfBedrooms = 3 },
                new Unit() { ID = 4, RealEstatePropertyID = 2, StatusID = 3, NumberOfBedrooms = 2 },
            };

            RealEstateProperties = new List<RealEstateProperty>()
            {
                new RealEstateProperty() { ID = 1, StatusID = 1, HasParallelPowerGeneration = true },
                new RealEstateProperty() { ID = 2, StatusID = 3, HasParallelPowerGeneration = false },
            };
        }

        protected IEnumerable<Status> Statuses { get; set; }
        protected IEnumerable<Unit> Units { get; set; }
        protected IEnumerable<RealEstateProperty> RealEstateProperties { get; set; }
    }
}
