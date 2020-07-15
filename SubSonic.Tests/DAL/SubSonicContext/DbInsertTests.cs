using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using SubSonic;
    using SubSonic.Schema;
    using System.Data;
    using System.Data.Common;
    using Models = Extensions.Test.Models;

    public partial class SubSonicContextTests
    {
        [Test]
        [Order(0)]
        public void ShouldBeAbleToInsertOnePersonRecordWithNoUDTT()
        {
            Models.Person person = GetFakePerson.Generate();
            
            Context.People.Add(person);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            Context.SaveChanges().Should().BeTrue();

            person.ID.Should().Be(personId);
            person.FullName.Should().Be(String.Format("{0}, {1}{2}",
                    person.FamilyName, person.FirstName,
                    string.IsNullOrEmpty(person.MiddleInitial?.Trim()) ? "" : $" {person.MiddleInitial}."));
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

            Context.Model.GetEntityModel<Models.Person>().DefinedTableTypeExists.Should().BeFalse();

            using (Context.Model.GetEntityModel<Models.Person>().AlteredState<IDbEntityModel, DbEntityModel>(new
            {
                DefinedTableType = new DbUserDefinedTableTypeAttribute(nameof(Models.Person))
            }).Apply())
            {
                Context.Model.GetEntityModel<Models.Person>().DefinedTableTypeExists.Should().BeTrue();

                Models.Person person = new Models.Person() { FirstName = "First_1", FamilyName = "Last_1", MiddleInitial = "M" };

                Context.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
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

                        data.ID = ++personId;

                        data.FullName = String.Format("{0}, {1}{2}",
                            data.FamilyName, data.FirstName,
                            data.MiddleInitial.IsNotNullOrEmpty() ? $" {data.MiddleInitial}." : "");

                        return new[] { data }.ToDataTable();
                    }

                    throw new NotSupportedException();
                });

                Context.People.Add(person);

                Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

                Context.SaveChanges().Should().BeTrue();

                person.ID.Should().Be(personId);
                person.FullName.Should().Be("Last_1, First_1 M.");
            }

            FluentActions.Invoking(() =>
                    Context.Database.Instance.RecievedCommand(expected_cmd))
                    .Should().NotThrow();

            Context.Database.Instance.RecievedCommandCount(expected_cmd)
                .Should()
                .Be(1);

            Context.Model.GetEntityModel<Models.Person>().DefinedTableTypeExists.Should().BeFalse();
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

            Context.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
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

                    data.ID = ++unitId;

                    return new[] { data }.ToDataTable();
                }

                throw new InvalidOperationException();
            });

            Context.Units.Add(unit);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(1);

            Context.SaveChanges().Should().BeTrue();

            unit.ID.Should().Be(unitId);
        }

        [Test]
        [Order(1)]
        public void ShouldBeAbleToInsertThreePeopleRecordsWithNoUDTT()
        {
            string expected_cmd = @"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO @output
VALUES
	(@FirstName, @MiddleInitial, @FamilyName)";

            Models.Person[] people = new[]
            {
                new Models.Person(){ FirstName = "First_2", FamilyName = "Last_2", MiddleInitial = "M" },
                new Models.Person(){ FirstName = "First_3", FamilyName = "Last_3", MiddleInitial = "", FullName = "Last_3, Last_3" },
                new Models.Person(){ FirstName = "First_4", FamilyName = "Last_4", FullName = "First_4 Last_4" }
            };

            Context.People.AddRange(people);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(3);

            Context.SaveChanges().Should().BeTrue();

            FluentActions.Invoking(() =>
                    Context.Database.Instance.RecievedCommand(expected_cmd))
                    .Should().NotThrow();

            Context.Database.Instance.RecievedCommandCount(expected_cmd)
                .Should()
                .Be(people.Length);

            people[0].ID.Should().Be(51);
            people[0].FullName.Should().Be("Last_2, First_2 M.");
            people[1].ID.Should().Be(52);
            people[1].FullName.Should().Be("Last_3, First_3");
            people[2].ID.Should().Be(53);
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

            Context.Database.Instance.AddCommandBehavior(expected_cmd, cmd =>
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

                        _data.ID = ++unitId;
                    }

                    return data.ToDataTable();
                }

                throw new InvalidOperationException();
            });

            Models.Unit[] units;

            Context.Units.AddRange(units = new[]
            {
                new Models.Unit() { NumberOfBedrooms = 2, RealEstatePropertyID = 1, StatusID = 1 },
                new Models.Unit() { NumberOfBedrooms = 3, RealEstatePropertyID = 1, StatusID = 1 }
            });

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(2);

            Context.SaveChanges().Should().BeTrue();

            units[0].ID.Should().Be(26);
            units[1].ID.Should().Be(27);
        }

        const string renter_expected_temp = @"INSERT INTO [dbo].[Renter]
OUTPUT INSERTED.* INTO @output
VALUES
	(@ID, @PersonID, @UnitID, @Rent, @StartDate, @EndDate)";

        const string renter_expected_udtt = @"INSERT INTO [dbo].[Renter]
OUTPUT INSERTED.* INTO @output
SELECT
	[ID],
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
                    new Models.Renter(){ ID = ++renterId, PersonID = 1, UnitID = 1, Rent = 450M, StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2019, 12, 31) },
                    new Models.Renter(){ ID = ++renterId, PersonID = 1, UnitID = 1, Rent = 500M, StartDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2020, 12, 31) },
                    new Models.Renter(){ ID = ++renterId, PersonID = 2, UnitID = 1, Rent = 450M, StartDate = new DateTime(2018, 1, 1), EndDate = new DateTime(2018, 12, 31) },
                    new Models.Renter(){ ID = ++renterId, PersonID = 2, UnitID = 2, Rent = 600M, StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2020, 12, 31) },
                }, 
                original = new Models.Renter[renters.Length];

            renters.CopyTo(original, 0);

            Context.Database.Instance.AddCommandBehavior(expected, cmd =>
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

            Context.Renters.AddRange(renters);

            Context.ChangeTracking.SelectMany(x => x.Value).Count(x => x.IsNew).Should().Be(renters.Length);

            if (withUDTT)
            {
                using (Context.Model.GetEntityModel<Models.Renter>().AlteredState<IDbEntityModel, DbEntityModel>(new
                {
                    DefinedTableType = new DbUserDefinedTableTypeAttribute(nameof(Models.Renter))
                }).Apply())
                {
                    Context.SaveChanges().Should().BeTrue();
                }
            }
            else
            {
                Context.SaveChanges().Should().BeTrue();
            }

            FluentActions.Invoking(() =>
                    Context.Database.Instance.RecievedCommand(expected))
                    .Should().NotThrow();

            Context.Database.Instance.RecievedCommandCount(expected)
                .Should()
                .Be(withUDTT ? 1 : original.Length);

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
            Models.Renter renter = new Models.Renter()
            {
                ID = (int)cmd.Parameters[$"@ID"].Value,
                PersonID = (int)cmd.Parameters[$"@PersonID"].Value,
                UnitID = (int)cmd.Parameters[$"@UnitID"].Value,
                Rent = (decimal)cmd.Parameters[$"@Rent"].Value,
                StartDate = (DateTime)cmd.Parameters[$"@StartDate"].Value,
                EndDate = (DateTime)cmd.Parameters[$"@EndDate"].Value
            };

            Context.Renters.Add(renter);

            return new[] { renter }.ToDataTable();
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
                            ID = (int)table.Rows[i][nameof(Models.Renter.ID)],
                            PersonID = (int)table.Rows[i][nameof(Models.Renter.PersonID)],
                            UnitID = (int)table.Rows[i][nameof(Models.Renter.UnitID)],
                            Rent = (decimal)table.Rows[i][nameof(Models.Renter.Rent)],
                            StartDate = (DateTime)table.Rows[i][nameof(Models.Renter.StartDate)],
                            EndDate = (DateTime)table.Rows[i][nameof(Models.Renter.EndDate)]
                        };

                        renters.Add(renter);
                    }

                    Context.Renters.AddRange(renters);

                    return renters.ToDataTable();
                }
            }

            throw new InvalidOperationException();
        }
    }
}
