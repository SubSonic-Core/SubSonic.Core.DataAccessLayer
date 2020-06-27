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
    using SubSonic.Extensions.Test.Data.Builders;
    using SubSonic.Linq;
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
                units_property =
            @"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = @RealEstatePropertyID)",
                unitsById = @"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[ID] = @id_1)",
                status =
            @"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @id_1)",
                statuses =
            @"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]",
                property =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[ID] = @id_1)",
                property_all =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]",
                people_all = @"SELECT [{0}].[ID], [{0}].[FirstName], [{0}].[MiddleInitial], [{0}].[FamilyName], [{0}].[FullName]
FROM [dbo].[Person] AS [{0}]",
                people_count = @"SELECT COUNT([{0}].[ID])
FROM [dbo].[Person] AS [{0}]",
                people_greater_than = @"SELECT [{0}].[ID], [{0}].[FirstName], [{0}].[MiddleInitial], [{0}].[FamilyName], [{0}].[FullName]
FROM [dbo].[Person] AS [{0}]
WHERE ([{0}].[ID] > @id_1)",
                people_greater_than_cnt = @"SELECT COUNT([{0}].[ID])
FROM [dbo].[Person] AS [{0}]
WHERE ([{0}].[ID] > @id_1)",
                person_count = @"SELECT COUNT([{0}].[ID])
FROM [dbo].[Person] AS [{0}]
WHERE ([{0}].[ID] = @id_1)",
                renters = @"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]",
                renters_filtered = @"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
WHERE (([{0}].[PersonID] = @personid_1) AND ([{0}].[UnitID] = @unitid_2))",
                renters_filtered_count = @"SELECT COUNT([T1].[PersonID])
FROM [dbo].[Renter] AS [T1]
WHERE (([T1].[PersonID] = @personid_1) AND ([T1].[UnitID] = @unitid_2))",
                renters_count = @"SELECT COUNT([{0}].[PersonID])
FROM [dbo].[Renter] AS [{0}]",
                person =
@"SELECT [{0}].[ID], [{0}].[FirstName], [{0}].[MiddleInitial], [{0}].[FamilyName], [{0}].[FullName]
FROM [dbo].[Person] AS [{0}]
WHERE ([{0}].[ID] = @id_1)";

            Context.Database.Instance.AddCommandBehavior(unitsById.Format("T1"), cmd => Units.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(units_property.Format("T1"), cmd => Units.Where(x => x.RealEstatePropertyID == cmd.Parameters["@RealEstatePropertyID"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(status.Format("T1"), cmd => 
                Statuses.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(statuses.Format("T1"), Statuses);
            Context.Database.Instance.AddCommandBehavior(property.Format("T1"), cmd => RealEstateProperties.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(property_all.Format("T1"), RealEstateProperties);
            Context.Database.Instance.AddCommandBehavior(people_all.Format("T1"), People);
            Context.Database.Instance.AddCommandBehavior(people_count.Format("T1"), cmd => new[] { People.Count }.ToDataTable());
            Context.Database.Instance.AddCommandBehavior(person.Format("T1"), cmd => People.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(person_count.Format("T1"), cmd => new[] { People.Count(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()) }.ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_greater_than.Format("T1"), cmd => People.Where(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_greater_than_cnt.Format("T1"), cmd => new[] { People.Count(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>()) }.ToDataTable());
            Context.Database.Instance.AddCommandBehavior(renters.Format("T1"), Renters);
            Context.Database.Instance.AddCommandBehavior(renters_filtered.Format("T1"), cmd => Renters.Where(x =>
                x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>() &&
                x.UnitID == cmd.Parameters["@unitid_2"].GetValue<int>())
            .ToDataTable());
            Context.Database.Instance.AddCommandBehavior(renters_filtered_count, cmd => new[]
            {
                Renters.Count(x =>
                    x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>() &&
                    x.UnitID == cmd.Parameters["@unitid_2"].GetValue<int>())
            }
            .ToDataTable());
            Context.Database.Instance.AddCommandBehavior(renters_count.Format("T1"), cmd => new[] { Renters.Count }.ToDataTable());
        }

        [Test]
        public void DbSetCollectionsShouldBeInitialized()
        {
            Context.RealEstateProperties.Should().NotBeNull();
            Context.Statuses.Should().NotBeNull();
            Context.Units.Should().NotBeNull();
        }

        [Test]
        public void DbOptionsShouldBeInitialized()
        {
            Context.Options.Should().NotBeNull();
        }

        [Test]
        public void DbModelShouldBeInitialized()
        {
            Context.Model.Should().NotBeNull();
        }

        [Test]
        public void ShouldBeAbleToCreateConnection()
        {
            DbConnection dbConnection = Context.Database.CreateConnection();

            dbConnection.Should().NotBeNull();
            dbConnection.ConnectionString.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void ShouldBeAbleToUpdateRecordsUsingCQRS()
        {
            string
                update = "[dbo].[UpdateRealEstateProperty]";

            Context.Database.Instance.AddCommandBehavior(update, (cmd) =>
            {
                if (cmd.Parameters[0].Value is DataTable data)
                {
                    data.Rows[0]["ID"].Should().Be(1);

                    return data;
                }

                throw new NotSupportedException();
            });

            Models.RealEstateProperty property = Context.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            property.HasParallelPowerGeneration = !(property.HasParallelPowerGeneration ?? false);

            ((IEntityProxy)property).IsDirty.Should().BeTrue();

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(1);

            Context.SaveChanges().Should().BeTrue();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            property = Context.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToUpdateMultipleRecordsUsingCQRS()
        {
            string
                update = "[dbo].[UpdateRealEstateProperty]";

            Context.Database.Instance.AddCommandBehavior(update, (cmd) =>
            {
                if (cmd.Parameters[0].Value is DataTable data)
                {
                    data.Rows.Count.Should().BeGreaterThan(1);

                    return data;
                }

                throw new NotSupportedException();
            });

            foreach (Models.RealEstateProperty property in Context.RealEstateProperties)
            {
                property.HasParallelPowerGeneration = !(property.HasParallelPowerGeneration ?? false);

                ((IEntityProxy)property).IsDirty.Should().BeTrue();
            }

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(Context.RealEstateProperties.Count);

            Context.SaveChanges().Should().BeTrue();

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToDeleteRecordsUsingCQRS()
        {
            string
                delete = "[dbo].[DeleteRealEstateProperty]";

            Context.Database.Instance.AddCommandBehavior(delete, (cmd) =>
            {
                if (cmd.Parameters[0].Value is DataTable data)
                {
                    data.Rows[0]["ID"].Should().Be(1);

                    return 0;
                }

                throw new NotSupportedException();
            });

            Models.RealEstateProperty property = Context.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            ((IEntityProxy)property).IsDeleted.Should().BeFalse();

            Context.RealEstateProperties.Delete(property);

            ((IEntityProxy)property).IsDeleted.Should().BeTrue();

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDeleted).Should().Be(1);

            Context.SaveChanges().Should().BeTrue();

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.KeyData.IsSameAs(new object[] { 1 })).Should().Be(0);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDeleted).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToInsertOneRealEstatePropertyRecordUsingCQRS()
        {
            string
                insert = "[dbo].[InsertRealEstateProperty]";

            Context.Database.Instance.AddCommandBehavior(insert, (cmd) =>
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

            Context.RealEstateProperties.Add(property);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            Context.SaveChanges().Should().BeTrue();

            property.ID.Should().Be(id);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToDetectWhenFailInsertRecordsUsingCQRS()
        {
            string
                insert = "[dbo].[InsertRealEstateProperty]";

            Context.Database.Instance.AddCommandBehavior(insert, (cmd) =>
            {
                DataTable correlation = null;

                foreach (DbParameter parameter in cmd.Parameters)
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

            Context.RealEstateProperties.Add(property);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            Context.SaveChanges().Should().BeFalse();

            property.ID.Should().Be(0);
            // bad data removed from change tracking.
            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToPageData()
        {
            int 
                recordCount = RealEstateProperties.Count(),
                pageSize = 3,
                pageCount = (int)Math.Ceiling((decimal)recordCount / pageSize);

            string
                count =
@"SELECT COUNT([T1].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [T1]",
                paged =
@"WITH page AS
(
	SELECT [T1].[ID]
	FROM [dbo].[RealEstateProperty] AS [T1]
	ORDER BY [T1].[ID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]
	INNER JOIN page
		ON ([page].[ID] = [T1].[ID])
OPTION (RECOMPILE)";

            Context.Database.Instance.AddCommandBehavior(count, (cmd) =>
            {
                using (DataTableBuilder table = new DataTableBuilder())
                {
                    table
                    .AddColumn("RECORDCOUNT", typeof(int))
                    .AddRow(RealEstateProperties.Count());

                    return table.DataTable;
                }
            });

            Func<DbCommand, DataTable> command = (cmd) =>
            {
                int size = 0, page = 0;

                foreach (DbParameter parameter in cmd.Parameters)
                {
                    if (parameter.ParameterName.Contains("PageSize"))
                    {
                        size = (int)parameter.Value;
                    }
                    else if (parameter.ParameterName.Contains("PageNumber"))
                    {
                        page = (int)parameter.Value;
                    }
                }

                return RealEstateProperties
                    .Skip(size * (page - 1))
                    .Take(size)
                    .ToDataTable();
            };

            Context.Database.Instance.AddCommandBehavior(paged, command);

            IDbPageCollection<Models.RealEstateProperty> collection = Context.RealEstateProperties.ToPagedCollection(pageSize);

            collection.PageSize.Should().Be(pageSize);

            collection.GetRecordsForPage(1).Count().Should()
                .BeGreaterOrEqualTo(pageSize)
                .And
                .BeLessThan(collection.RecordCount);

            collection.PageNumber.Should().Be(1);

            collection.GetRecordsForPage(2).Count().Should().Be(1);
            collection.PageNumber.Should().Be(2);

            collection.RecordCount.Should().Be(recordCount);
            collection.PageCount.Should().Be(pageCount);
        }

        [Test]
        public void ShouldBeAbleToEnumeratePagedData()
        {
            int
                recordCount = RealEstateProperties.Count(),
                pageSize = 3,
                pageCount = (int)Math.Ceiling((decimal)recordCount / pageSize);

            string
                count =
@"SELECT COUNT([T1].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [T1]",
                paged =
@"WITH page AS
(
	SELECT [T1].[ID]
	FROM [dbo].[RealEstateProperty] AS [T1]
	ORDER BY [T1].[ID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]
	INNER JOIN page
		ON ([page].[ID] = [T1].[ID])
OPTION (RECOMPILE)";

            Context.Database.Instance.AddCommandBehavior(count, (cmd) =>
            {
                using (DataTableBuilder table = new DataTableBuilder())
                {
                    table
                    .AddColumn("RECORDCOUNT", typeof(int))
                    .AddRow(RealEstateProperties.Count());

                    return table.DataTable;
                }
            });

            Func<DbCommand, DataTable> command = (cmd) =>
            {
                int size = 0, page = 0;

                foreach (DbParameter parameter in cmd.Parameters)
                {
                    if (parameter.ParameterName.Contains("PageSize"))
                    {
                        size = (int)parameter.Value;
                    }
                    else if (parameter.ParameterName.Contains("PageNumber"))
                    {
                        page = (int)parameter.Value;
                    }
                }

                return RealEstateProperties
                    .Skip(size * (page - 1))
                    .Take(size)
                    .ToDataTable();
            };

            Context.Database.Instance.AddCommandBehavior(paged, command);

            IDbPageCollection<Models.RealEstateProperty> collection = Context.RealEstateProperties.ToPagedCollection(pageSize);

            bool enumerated = false;

            foreach(IDbPageCollection<Models.RealEstateProperty> page in collection.GetPages())
            {
                enumerated |= true;

                page.RecordCount.Should().Be(recordCount);

                if (page.PageNumber == 1)
                {
                    page.Count().Should()
                        .Be(pageSize)
                        .And
                        .BeLessThan(page.RecordCount);
                }
                else if (page.PageNumber == 2)
                {
                    page.Count().Should()
                        .Be(1)
                        .And
                        .BeLessThan(page.RecordCount);
                }
            }

            enumerated.Should().BeTrue();
        }
    }
}
