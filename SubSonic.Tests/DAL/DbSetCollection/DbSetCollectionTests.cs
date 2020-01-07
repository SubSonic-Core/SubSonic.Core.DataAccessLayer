using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using SubSonic.Linq;

namespace SubSonic.Tests.DAL.DbSetCollection
{
    using SubSonic.Data.Caching;
    using SubSonic.Data.DynamicProxies;
    using SUT;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    [TestFixture]
    public class DbSetCollectionTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            string
                units =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = {1})",
                status =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = {1})",
                statuses =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]";

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 0), Units.Where(x => x.ID == 0));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 1), Statuses.Where(x => x.ID == 1));
            DbContext.Database.Instance.AddCommandBehavior(statuses.Format("T1"), Statuses);
        }

        [Test]
        public void CanAddNewInstanceToCollection()
        {
            Status status = DbContext.Statuses.Single(x => x.ID == 1);

            RealEstateProperty property = new RealEstateProperty()
            {
                HasParallelPowerGeneration = true,
                StatusID = status.ID,
                Status = status
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
            SubSonic.DbContext.ChangeControl.Add(typeof(RealEstateProperty), new Entity<RealEstateProperty>(new RealEstateProperty() { ID = -1, StatusID = 1 }));

            SubSonic.DbContext.ChangeControl.Add(typeof(RealEstateProperty), DynamicProxy.MapInstanceOf(DbContext, new Entity<RealEstateProperty>(new RealEstateProperty() { ID = -2, StatusID = 1 })));

            SubSonic.DbContext.ChangeControl.Add(typeof(Status), DynamicProxy.MapInstanceOf(DbContext, new Entity<Status>(new Status() { ID = -1, Name = "None", IsAvailableStatus = false })));

            foreach (var item in SubSonic.DbContext.ChangeControl)
            {
                foreach (IEntityProxy proxy in item.Value)
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

        [Test]
        public void CanQueryFromCacheObject()
        {
            Expression expression = DbContext.Statuses.Where(x => x.ID == 1).Expression;

            Status
                status_ctrl = DbContext.Statuses.Where(x => x.ID == 1).Single(),
                status_cache = SubSonic.DbContext.ChangeControl.Where<IEnumerable<Status>>(typeof(Status), DbContext.Statuses.Provider, expression).Single();

            status_ctrl.Should().BeSameAs(status_cache);
        }

        [Test]
        public void CanPullFromCacheInsteadOfDatabase()
        {
            List<Status> statuses = DbContext.Statuses.ToList();

            foreach (Status status in statuses)
            {
                status.Should().NotBeNull();
            }

            Status _status = DbContext.Statuses.Single(x => x.ID == 2);

            _status.Should().NotBeNull();
            _status.ID.Should().Be(2);
        }
    }
}
