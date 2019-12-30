using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using SubSonic.Linq;

namespace SubSonic.Tests.DAL
{
    using SubSonic.Data.Caching;
    using SubSonic.Data.DynamicProxies;
    using SUT;

    [TestFixture]
    public class DbSetCollectionTests
        : BaseTestFixture
    {
        [Test]
        public void CanAddNewInstanceToCollection()
        {
            string
                units =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = {1})",
                status =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = {1})";

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 0), Units.Where(x => x.ID == 0));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 1), Statuses.Where(x => x.ID == 1));

            Status _status = DbContext.Statuses.Single(x => x.ID == 1);

            RealEstateProperty property = new RealEstateProperty()
            {
                HasParallelPowerGeneration = true,
                StatusID = _status.ID,
                Status = _status
            };

            DbContext.RealEstateProperties.Add(property);

            IEntityProxy<RealEstateProperty> proxy = (IEntityProxy<RealEstateProperty>)DbContext.RealEstateProperties.ElementAt(0);

            proxy.IsNew.Should().BeTrue();
            proxy.Data.HasParallelPowerGeneration.Should().Be(property.HasParallelPowerGeneration);
            proxy.Data.StatusID.Should().Be(property.StatusID);
            property.Should().BeEquivalentTo(proxy.Data);
        }

        [Test]
        public void CanEnumerateCacheObject()
        {
            SubSonic.DbContext.Cache.Add(typeof(RealEstateProperty), new Entity<RealEstateProperty>(new RealEstateProperty() { ID = -1, StatusID = 1 }));

            SubSonic.DbContext.Cache.Add(typeof(RealEstateProperty), DynamicProxy.MapInstanceOf(DbContext, new Entity<RealEstateProperty>(new RealEstateProperty() { ID = -2, StatusID = 1 })));

            SubSonic.DbContext.Cache.Add(typeof(Status), DynamicProxy.MapInstanceOf(DbContext, new Entity<Status>(new Status() { ID = -1, Name = "None", IsAvailableStatus = false })));

            foreach (var item in SubSonic.DbContext.Cache)
            {
                foreach(IEntityProxy proxy in item.Value)
                {
                    object value = null;
                    
                    if (proxy is Entity entity)
                    {
                        value = entity.Data;
                    }
                    
                    if ((value ?? proxy) is RealEstateProperty property)
                    {
                        property.Should().NotBeNull();
                    }
                    else if ((value ?? proxy) is Status status)
                    {
                        status.Should().NotBeNull();
                    }
                }
            }
        }
    }
}
