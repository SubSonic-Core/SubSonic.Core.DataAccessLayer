using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using SubSonic.Linq;

namespace SubSonic.Tests.DAL
{
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
            foreach(var item in SubSonic.DbContext.Cache)
            {

            }
        }
    }
}
