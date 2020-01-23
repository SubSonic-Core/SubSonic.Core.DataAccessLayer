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
WHERE ([{0}].[ID] = {1})",
                property_all =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]";

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 0), Units.Where(x => x.ID == 0));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 1), Statuses.Where(x => x.ID == 1));
            DbContext.Database.Instance.AddCommandBehavior(statuses.Format("T1"), Statuses);
            DbContext.Database.Instance.AddCommandBehavior(property.Format("T1", 1), RealEstateProperties.Where(x => x.ID == 1));
            DbContext.Database.Instance.AddCommandBehavior(property_all.Format("T1"), RealEstateProperties);
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
                update = "[dbo].[UpdateRealEstateProperty]";

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

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            property = DbContext.RealEstateProperties
                .Where(x => x.ID == 1)
                .Single();

            ((IEntityProxy)property).IsDirty.Should().BeFalse();

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToUpdateMultipleRecordsUsingCQRS()
        {
            string
                update = "[dbo].[UpdateRealEstateProperty]";

            DbContext.Database.Instance.AddCommandBehavior(update, (cmd) =>
            {
                if (cmd.Parameters[0].Value is DataTable data)
                {
                    data.Rows.Count.Should().BeGreaterThan(1);

                    return data;
                }

                throw new NotSupportedException();
            });

            foreach (Models.RealEstateProperty property in DbContext.RealEstateProperties)
            {
                property.HasParallelPowerGeneration = !(property.HasParallelPowerGeneration ?? false);

                ((IEntityProxy)property).IsDirty.Should().BeTrue();
            }

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(DbContext.RealEstateProperties.Count());

            DbContext.SaveChanges().Should().BeTrue();

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDirty).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToDeleteRecordsUsingCQRS()
        {
            string
                delete = "[dbo].[DeleteRealEstateProperty]";

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

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDeleted).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.KeyData.IsSameAs(new object[] { 1 })).Should().Be(0);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsDeleted).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToInsertRecordsUsingCQRS()
        {
            string
                insert = "[dbo].[InsertRealEstateProperty]";

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

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            property.ID.Should().Be(id);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(0);
        }

        [Test]
        public void ShouldBeAbleToDetectWhenFailInsertRecordsUsingCQRS()
        {
            string
                insert = "[dbo].[InsertRealEstateProperty]";

            DbContext.Database.Instance.AddCommandBehavior(insert, (cmd) =>
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

            DbContext.RealEstateProperties.Add(property);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            DbContext.SaveChanges().Should().BeFalse();

            property.ID.Should().Be(0);
            // bad data removed from change tracking.
            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(0);
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
	OFFSET {0} * ({1} - 1) ROWS
	FETCH NEXT {0} ROWS ONLY
)
SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]
	INNER JOIN page
		ON ([page].[ID] = [T1].[ID])
OPTION (RECOMPILE)";

            DbContext.Database.Instance.AddCommandBehavior(count, (cmd) =>
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

            DbContext.Database.Instance.AddCommandBehavior(paged.Format(pageSize, 1), command);
            DbContext.Database.Instance.AddCommandBehavior(paged.Format(pageSize, 2), command);

            IDbPageCollection<Models.RealEstateProperty> collection = DbContext.RealEstateProperties.ToPagedCollection(pageSize);

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
	OFFSET {0} * ({1} - 1) ROWS
	FETCH NEXT {0} ROWS ONLY
)
SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]
	INNER JOIN page
		ON ([page].[ID] = [T1].[ID])
OPTION (RECOMPILE)";

            DbContext.Database.Instance.AddCommandBehavior(count, (cmd) =>
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

            DbContext.Database.Instance.AddCommandBehavior(paged.Format(pageSize, 1), command);
            DbContext.Database.Instance.AddCommandBehavior(paged.Format(pageSize, 2), command);

            IDbPageCollection<Models.RealEstateProperty> collection = DbContext.RealEstateProperties.ToPagedCollection(pageSize);

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
