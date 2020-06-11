using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using Models = Extensions.Test.Models;

    public partial class DbContextTests
    {
        [Test()]
        [Order(0)]
        public void ShouldBeAbleToInsertOnePersonRecordWithNoUDTT()
        {
            string expected_cmd = @"CREATE TABLE #Person(

    ID INT,
    FirstName varchar(50) NOT NULL,
    MiddleInitial varchar(1) NULL,
	FamilyName varchar(50) NOT NULL,
    FullName varchar(104) NOT NULL
);

INSERT INTO [dbo].[Person]
OUTPUT inserted.*INTO #Person
VALUES
    (@FirstName_1, @MiddleInitial_1, @FamilyName_1);

SELECT
    ID,
    FirstName,
    MiddleInitial,
    FamilyName,
    FullName
FROM #Person;

DROP TABLE #Person;";

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
        [Order(1)]
        public void ShouldBeAbleToInsertThreePeopleRecordsWithNoUDTT()
        {
            string expected_cmd = @"CREATE TABLE #Person(

    ID INT,
    FirstName varchar(50) NOT NULL,
    MiddleInitial varchar(1) NULL,
	FamilyName varchar(50) NOT NULL,
    FullName varchar(104) NOT NULL
);

INSERT INTO [dbo].[Person]
OUTPUT inserted.*INTO #Person
VALUES
    (@FirstName_1, @MiddleInitial_1, @FamilyName_1),
    (@FirstName_2, @MiddleInitial_2, @FamilyName_2),
    (@FirstName_3, @MiddleInitial_3, @FamilyName_3);

SELECT
    ID,
    FirstName,
    MiddleInitial,
    FamilyName,
    FullName
FROM #Person;

DROP TABLE #Person;";

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

            people[0].ID.Should().Be(2);
            people[0].FullName.Should().Be("Last_2, First_2 M.");
            people[1].ID.Should().Be(3);
            people[1].FullName.Should().Be("Last_3, First_3");
            people[2].ID.Should().Be(4);
            people[2].FullName.Should().Be("Last_4, First_4");
        }
    }
}
