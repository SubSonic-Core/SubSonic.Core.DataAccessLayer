using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using SubSonic.Infrastructure;
    using SubSonic.Infrastructure.Schema;
    using System.Data;
    using System.Data.Common;
    using Models = Extensions.Test.Models;

    public partial class DbContextTests
    {
        [Test]
        [Order(0)]
        public void ShouldBeAbleToInsertOnePersonRecordWithNoUDTT()
        {
            string expected_cmd = @"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO #Person
VALUES
	('First_1', 'M', 'Last_1')";

            Models.Person person = new Models.Person(){ FirstName = "First_1", FamilyName = "Last_1", MiddleInitial = "M" };

            DbContext.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
            {
                Models.Person data = new Models.Person()
                {
                    FamilyName = cmd.Parameters["@FamilyName_1"].Value.ToString(),
                    FirstName = cmd.Parameters["@FirstName_1"].Value.ToString(),
                    MiddleInitial = cmd.Parameters["@MiddleInitial_1"].Value.IsNotNull(x => x.ToString())
                };

                People.Add(data);
                
                data.ID = People.Count;

                data.FullName = String.Format("{0}, {1}{2}",
                    data.FamilyName, data.FirstName,
                    data.MiddleInitial.IsNotNullOrEmpty() ? $" {data.MiddleInitial}." : "");

                return new[] { data }.ToDataTable();
            });

            DbContext.People.Add(person);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            person.ID.Should().Be(1);
            person.FullName.Should().Be("Last_1, First_1 M.");
        }

        [Test]
        [Order(0)]
        public void ShouldBeAbleToInsertOnePersonRecordWithUDTT()
        {
            string expected_cmd = @"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO @output
SELECT
	[FirstName],
	[MiddleInitial],
	[FamilyName]
FROM @input";

            DbContext.Model.GetEntityModel<Models.Person>().DefinedTableTypeExists.Should().BeFalse();

            using (DbContext.Model.GetEntityModel<Models.Person>().AlteredState<IDbEntityModel, DbEntityModel>(new
            {
                DefinedTableType = new DbUserDefinedTableTypeAttribute(nameof(Models.Person))
            }).Apply())
            {
                DbContext.Model.GetEntityModel<Models.Person>().DefinedTableTypeExists.Should().BeTrue();

                Models.Person person = new Models.Person() { FirstName = "First_1", FamilyName = "Last_1", MiddleInitial = "M" };

                DbContext.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
                {
                    if (cmd.Parameters["@input"].Value is DataTable table)
                    {
                        Models.Person data = new Models.Person()
                        {
                            FirstName = (string)table.Rows[0]["FirstName"],
                            MiddleInitial = (string)table.Rows[0]["MiddleInitial"],
                            FamilyName = (string)table.Rows[0]["FamilyName"]
                        };

                        People.Add(data);

                        data.ID = People.Count;

                        data.FullName = String.Format("{0}, {1}{2}",
                            data.FamilyName, data.FirstName,
                            data.MiddleInitial.IsNotNullOrEmpty() ? $" {data.MiddleInitial}." : "");

                        return new[] { data }.ToDataTable();
                    }

                    throw new NotSupportedException();
                });

                DbContext.People.Add(person);

                DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

                DbContext.SaveChanges().Should().BeTrue();

                person.ID.Should().Be(1);
                person.FullName.Should().Be("Last_1, First_1 M.");
            }

            DbContext.Model.GetEntityModel<Models.Person>().DefinedTableTypeExists.Should().BeFalse();
        }

        [Test]
        [Order(0)]
        public void ShouldBeAbleToInsertOneUnitRecordWithUDTT()
        {
            string expected_cmd = @"INSERT INTO [dbo].[Unit]
OUTPUT INSERTED.* INTO @output
SELECT
	[Bedrooms],
	[StatusID],
	[RealEstatePropertyID]
FROM @input";

            Models.Unit unit = new Models.Unit() { NumberOfBedrooms = 2, RealEstatePropertyID = 1, StatusID = 1 };

            DbContext.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
            {
                if (cmd.Parameters["@input"].Value is DataTable table)
                {
                    Models.Unit data = new Models.Unit()
                    {
                        NumberOfBedrooms = (int)table.Rows[0]["Bedrooms"],
                        StatusID = (int)table.Rows[0]["StatusID"],
                        RealEstatePropertyID = (int)table.Rows[0]["RealEstatePropertyID"]
                    };

                    Units.Add(data);

                    data.ID = Units.Count;

                    return new[] { data }.ToDataTable();
                }

                throw new InvalidOperationException();
            });

            DbContext.Units.Add(unit);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            DbContext.SaveChanges().Should().BeTrue();

            unit.ID.Should().Be(5);
        }

        [Test]
        [Order(1)]
        public void ShouldBeAbleToInsertThreePeopleRecordsWithNoUDTT()
        {
            string expected_cmd = @"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO #Person
VALUES
	('First_2', 'M', 'Last_2'),
	('First_3', '', 'Last_3'),
	('First_4', , 'Last_4')";

            Models.Person[] people = new[]
            {
                new Models.Person(){ FirstName = "First_2", FamilyName = "Last_2", MiddleInitial = "M" },
                new Models.Person(){ FirstName = "First_3", FamilyName = "Last_3", MiddleInitial = "", FullName = "Last_3, Last_3" },
                new Models.Person(){ FirstName = "First_4", FamilyName = "Last_4", FullName = "First_4 Last_4" }
            };

            DbContext.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
            {
                cmd.Parameters["@MiddleInitial_1"].DbType.Should().Be(DbType.AnsiString);
                cmd.Parameters["@MiddleInitial_2"].DbType.Should().Be(DbType.AnsiString);
                cmd.Parameters["@MiddleInitial_3"].DbType.Should().Be(DbType.AnsiString);

                Models.Person[] _persons = new[]
                {
                    new Models.Person()
                    {
                        FamilyName = cmd.Parameters["@FamilyName_1"].Value.ToString(),
                        FirstName = cmd.Parameters["@FirstName_1"].Value.ToString(),
                        MiddleInitial = cmd.Parameters["@MiddleInitial_1"].Value.IsNotNull(x => x.ToString())
                    },
                    new Models.Person()
                    {
                        FamilyName = cmd.Parameters["@FamilyName_2"].Value.ToString(),
                        FirstName = cmd.Parameters["@FirstName_2"].Value.ToString(),
                        MiddleInitial = cmd.Parameters["@MiddleInitial_2"].Value.IsNotNull(x => x.ToString())
                    },
                    new Models.Person()
                    {
                        FamilyName = cmd.Parameters["@FamilyName_3"].Value.ToString(),
                        FirstName = cmd.Parameters["@FirstName_3"].Value.ToString(),
                        MiddleInitial = cmd.Parameters["@MiddleInitial_3"].Value.IsNotNull(x => x.ToString())
                    }
                };

                foreach (var person in _persons)
                {
                    People.Add(person);

                    person.ID = People.Count;
                    person.FullName = String.Format("{0}, {1}{2}",
                        person.FamilyName, person.FirstName,
                        person.MiddleInitial.IsNotNullOrEmpty() ? $" {person.MiddleInitial}." : "");
                }

                return _persons.ToDataTable();
            });

            DbContext.People.AddRange(people);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(3);

            DbContext.SaveChanges().Should().BeTrue();

            people[0].ID.Should().Be(1);
            people[0].FullName.Should().Be("Last_2, First_2 M.");
            people[1].ID.Should().Be(2);
            people[1].FullName.Should().Be("Last_3, First_3");
            people[2].ID.Should().Be(3);
            people[2].FullName.Should().Be("Last_4, First_4");
        }

        [Test]
        [Order(1)]
        public void ShouldBeAbleToInsertTwoUnitRecordWithUDTT()
        {
            string expected_cmd = @"INSERT INTO [dbo].[Unit]
OUTPUT INSERTED.* INTO @output
SELECT
	[Bedrooms],
	[StatusID],
	[RealEstatePropertyID]
FROM @input";

            DbContext.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
            {
                if (cmd.Parameters["@input"].Value is DataTable table)
                {
                    Models.Unit[] data = new[]
                    {
                        new Models.Unit()
                        {
                            NumberOfBedrooms = (int)table.Rows[0]["Bedrooms"],
                            StatusID = (int)table.Rows[0]["StatusID"],
                            RealEstatePropertyID = (int)table.Rows[0]["RealEstatePropertyID"]
                        },
                        new Models.Unit()
                        {
                            NumberOfBedrooms = (int)table.Rows[1]["Bedrooms"],
                            StatusID = (int)table.Rows[1]["StatusID"],
                            RealEstatePropertyID = (int)table.Rows[1]["RealEstatePropertyID"]
                        }
                    };

                    foreach (var _data in data)
                    {
                        Units.Add(_data);

                        _data.ID = Units.Count;
                    }

                    return data.ToDataTable();
                }

                throw new InvalidOperationException();
            });

            Models.Unit[] units;

            DbContext.Units.AddRange(units = new[]
            {
                new Models.Unit() { NumberOfBedrooms = 2, RealEstatePropertyID = 1, StatusID = 1 },
                new Models.Unit() { NumberOfBedrooms = 3, RealEstatePropertyID = 1, StatusID = 1 }
            });

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(2);

            DbContext.SaveChanges().Should().BeTrue();

            units[0].ID.Should().Be(5);
            units[1].ID.Should().Be(6);
        }

        const string renter_expected_temp = @"INSERT INTO [dbo].[Renter]
OUTPUT INSERTED.* INTO #Renter
VALUES
	(1, 1, 450, 1/1/2019 12:00:00 AM, 12/31/2019 12:00:00 AM),
	(1, 1, 500, 1/1/2020 12:00:00 AM, 12/31/2020 12:00:00 AM),
	(2, 1, 450, 1/1/2018 12:00:00 AM, 12/31/2018 12:00:00 AM),
	(2, 2, 600, 1/1/2019 12:00:00 AM, 12/31/2020 12:00:00 AM)";

        const string renter_expected_udtt = @"INSERT INTO [dbo].[Renter]
OUTPUT INSERTED.* INTO @output
SELECT
	[PersonID],
	[UnitID],
	[Rent],
	[StartDate],
	[EndDate]
FROM @input";

        [Test]
        [Order(2)]
        [TestCase(false, renter_expected_temp)]
        [TestCase(true, renter_expected_udtt)]
        public void ShouldBeAbleToInsertRenterWithCompositeKey(bool withUDTT, string expected)
        {
            Models.Renter[] 
                renters = new[]
                {
                    new Models.Renter(){ PersonID = 1, UnitID = 1, Rent = 450M, StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2019, 12, 31) },
                    new Models.Renter(){ PersonID = 1, UnitID = 1, Rent = 500M, StartDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2020, 12, 31) },
                    new Models.Renter(){ PersonID = 2, UnitID = 1, Rent = 450M, StartDate = new DateTime(2018, 1, 1), EndDate = new DateTime(2018, 12, 31) },
                    new Models.Renter(){ PersonID = 2, UnitID = 2, Rent = 600M, StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2020, 12, 31) },
                }, 
                original = new Models.Renter[renters.Length];

            renters.CopyTo(original, 0);

            DbContext.Database.Instance.AddCommandBehavior(expected, cmd =>
            {
                if (withUDTT)
                {
                    return RenterCmdBehaviorForUDTT(cmd, renters.Length);
                }
                else
                {
                    return RenterCmdBehaviorForTempTable(cmd, renters.Length);
                }
            });

            DbContext.Renters.AddRange(renters);

            DbContext.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(renters.Length);

            AlteredState<IDbEntityModel, DbEntityModel> state = null;

            if (withUDTT)
            {
                state = DbContext.Model.GetEntityModel<Models.Renter>().AlteredState<IDbEntityModel, DbEntityModel>(new
                {
                    DefinedTableType = new DbUserDefinedTableTypeAttribute(nameof(Models.Renter))
                }).Apply();
            }

            DbContext.SaveChanges().Should().BeTrue();

            if (withUDTT)
            {
                state.Dispose();
                state = null;
            }

            for (int i = 0; i < original.Length; i++)
            {
                renters[i].PersonID.Should().Be(original[i].PersonID);
                renters[i].UnitID.Should().Be(original[i].UnitID);
                renters[i].Rent.Should().Be(original[i].Rent);
                renters[i].StartDate.Should().Be(original[i].StartDate);
                renters[i].EndDate.Should().Be(original[i].EndDate);
            }
        }

        private DataTable RenterCmdBehaviorForTempTable(DbCommand cmd, int count)
        {
            List<Models.Renter> renters = new List<Models.Renter>();

            for(int i = 0; i < count; i++)
            {
                int x = i + 1;

                Models.Renter renter = new Models.Renter()
                {
                    PersonID = (int)cmd.Parameters[$"@PersonID_{x}"].Value,
                    UnitID = (int)cmd.Parameters[$"@UnitID_{x}"].Value,
                    Rent = (decimal)cmd.Parameters[$"@Rent_{x}"].Value,
                    StartDate = (DateTime)cmd.Parameters[$"@StartDate_{x}"].Value,
                    EndDate = (DateTime)cmd.Parameters[$"@EndDate_{x}"].Value
                };

                renters.Add(renter);
            }

            DbContext.Renters.AddRange(renters);

            return renters.ToDataTable();
        }

        private DataTable RenterCmdBehaviorForUDTT(DbCommand cmd, int count)
        {
            if (cmd.Parameters["@input"].Value is DataTable table)
            {
                using (table)
                {
                    List<Models.Renter> renters = new List<Models.Renter>();

                    for (int i = 0; i < count; i++)
                    {
                        Models.Renter renter = new Models.Renter()
                        {
                            PersonID = (int)table.Rows[i]["PersonID"],
                            UnitID = (int)table.Rows[i]["UnitID"],
                            Rent = (decimal)table.Rows[i]["Rent"],
                            StartDate = (DateTime)table.Rows[i]["StartDate"],
                            EndDate = (DateTime)table.Rows[i]["EndDate"]
                        };

                        renters.Add(renter);
                    }

                    DbContext.Renters.AddRange(renters);

                    return renters.ToDataTable();
                }
            }

            throw new InvalidOperationException();
        }
    }
}
