using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Infrastructure;
using System.Data.Common;
using System.Linq;

namespace SubSonic.Tests.DAL
{
    using SUT;
    using Models = Extensions.Test.Models;
    //using Linq;

    [TestFixture]
    public partial class DbContextTests
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
FROM [dbo].[Status] AS [{0}]",
                property =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[ID] = {1})";

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 0), Units.Where(x => x.ID == 0));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 1), Statuses.Where(x => x.ID == 1));
            DbContext.Database.Instance.AddCommandBehavior(statuses.Format("T1"), Statuses);
            DbContext.Database.Instance.AddCommandBehavior(property.Format("T1", 1), RealEstateProperties.Where(x => x.ID == 1));
        }

        [Test]
        public void DbSetCollectionsShouldBeInitialized()
        {
            DbContext.RealEstateProperties.Should().NotBeNull();
            DbContext.Statuses.Should().NotBeNull();
            DbContext.Units.Should().NotBeNull();
        }

        [Test]
        public void DbOptionsShouldBeInitialized()
        {
            DbContext.Options.Should().NotBeNull();
        }

        [Test]
        public void DbModelShouldBeInitialized()
        {
            DbContext.Model.Should().NotBeNull();
        }

        [Test]
        public void ShouldBeAbleToCreateConnection()
        {
            DbConnection dbConnection = DbContext.Database.CreateConnection();

            dbConnection.Should().NotBeNull();
            dbConnection.ConnectionString.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void ShouldBeAbleToSaveChangesToTheDatabaseContext()
        {
            string
                update =
@"EXEC [dbo].[UpdateRealEstateProperty] @Properties = @Properties";

            DbContext.Database.Instance.AddCommandBehavior(update, (cmd) =>
            {
                return 0;
            });

            Models.RealEstateProperty property = DbContext.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            property.HasParallelPowerGeneration = !(property.HasParallelPowerGeneration ?? false);

            ((IEntityProxy)property).IsDirty.Should().BeTrue();

            DbContext.SaveChanges().Should().BeTrue();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            property = DbContext.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();
        }
    }
}
