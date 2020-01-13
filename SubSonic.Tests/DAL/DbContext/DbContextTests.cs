using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Infrastructure;
using System.Data.Common;
using System.Linq;
using System.Data;
using System;

namespace SubSonic.Tests.DAL
{
    using SUT;
    using Models = Extensions.Test.Models;

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
        public void ShouldBeAbleToUpdateRecordsUsingCQRS()
        {
            string
                update =
@"EXEC @Result = [dbo].[UpdateRealEstateProperty] @Properties = @Properties";

            DbContext.Database.Instance.AddCommandBehavior(update, (cmd) =>
            {
                if (cmd.Parameters[0].Value is DataTable data)
                {
                    data.Rows[0]["ID"].Should().Be(1);

                    return data;
                }

                throw new NotSupportedException();
            });

            Models.RealEstateProperty property = DbContext.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            property.HasParallelPowerGeneration = !(property.HasParallelPowerGeneration ?? false);

            ((IEntityProxy)property).IsDirty.Should().BeTrue();

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            property = DbContext.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToDeleteRecordsUsingCQRS()
        {
            string
                delete =
@"EXEC @Result = [dbo].[DeleteRealEstateProperty] @Properties = @Properties";

            DbContext.Database.Instance.AddCommandBehavior(delete, (cmd) =>
            {
                if (cmd.Parameters[0].Value is DataTable data)
                {
                    data.Rows[0]["ID"].Should().Be(1);

                    return 0;
                }

                throw new NotSupportedException();
            });

            Models.RealEstateProperty property = DbContext.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            ((IEntityProxy)property).IsDeleted.Should().BeFalse();

            DbContext.RealEstateProperties.Delete(property);

            ((IEntityProxy)property).IsDeleted.Should().BeTrue();

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDeleted).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.KeyData.IsSameAs(new object[] { 1 })).Should().Be(0);

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDeleted).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToInsertRecordsUsingCQRS()
        {
            string
                insert =
@"EXEC @Result = [dbo].[InsertRealEstateProperty] @Properties = @Properties";

            DbContext.Database.Instance.AddCommandBehavior(insert, (cmd) =>
            {
                DataTable correlation = null;

                foreach (DbParameter parameter in cmd.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.ReturnValue)
                    {
                        parameter.Value = 0;
                    }
                    else
                    {
                        if (parameter.Value is DataTable properties)
                        {
                            correlation = properties;

                            properties.Rows[0]["ID"].Should().Be(0);
                            properties.Rows[0]["StatusID"].Should().Be(1);
                            properties.Rows[0]["HasParallelPowerGeneration"].Should().Be(true);

                            correlation.Rows[0]["ID"] = RealEstateProperties.Count() + 1;
                        }
                    }
                }

                return correlation;
            });

            int id = RealEstateProperties.Count() + 1;

            Models.RealEstateProperty property = new Models.RealEstateProperty()
            {
                StatusID = 1,
                HasParallelPowerGeneration = true
            };

            DbContext.RealEstateProperties.Add(property);

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            property.ID.Should().Be(id);

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToDetectWhenFailInsertRecordsUsingCQRS()
        {
            string
                insert =
@"EXEC @Result = [dbo].[InsertRealEstateProperty] @Properties = @Properties";

            DbContext.Database.Instance.AddCommandBehavior(insert, (cmd) =>
            {
                DataTable correlation = null;

                foreach(DbParameter parameter in cmd.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.ReturnValue)
                    {
                        parameter.Value = -1;
                    }
                    else
                    {
                        if (parameter.Value is DataTable properties)
                        {
                            correlation = properties;

                            correlation.Clear(); // nothing was inserted because of transaction rollback
                        }
                    }
                }

                return correlation;
            });

            int id = RealEstateProperties.Count() + 1;

            Models.RealEstateProperty property = new Models.RealEstateProperty()
            {
                StatusID = 1,
                HasParallelPowerGeneration = true
            };

            DbContext.RealEstateProperties.Add(property);

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            DbContext.SaveChanges().Should().BeFalse();

            property.ID.Should().Be(0);

            SubSonic.DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);
        }
    }
}
