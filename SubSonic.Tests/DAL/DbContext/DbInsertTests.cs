using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using System.Data;
    using Models = Extensions.Test.Models;

    public partial class DbContextTests
    {
        [Test()]
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

        [Test()]
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

        [Test()]
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

        [Test()]
        [Order(0)]
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
    }
}
