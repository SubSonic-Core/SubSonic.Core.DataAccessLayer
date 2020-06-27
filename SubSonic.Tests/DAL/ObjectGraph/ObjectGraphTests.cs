using NUnit.Framework;
using System;
using FluentAssertions;

namespace SubSonic.Tests.DAL.ObjectGraph
{
    using Infrastructure;
    using Extensions.Test;
    using SUT;
    using System.Linq;
    using Models = Extensions.Test.Models;

    [TestFixture]
    public class ObjectGraphTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            Context.Database.Instance.AddCommandBehavior(@"SELECT [T1].[ID], [T1].[Bedrooms] AS [NumberOfBedrooms], [T1].[StatusID], [T1].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [T1]
WHERE ([T1].[ID] = @id_1)", cmd => Units
                .Where(unit =>
                    unit.ID == cmd.Parameters["@id_1"].GetValue<int>())
                .ToDataTable());

            Context.Database.Instance.AddCommandBehavior(@"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO @output
VALUES
	(@FirstName, @MiddleInitial, @FamilyName)", cmd =>
            {
                Models.Person person = new Models.Person()
                {
                    FirstName = cmd.Parameters[$"@{nameof(Models.Person.FirstName)}"].GetValue<string>(),
                    MiddleInitial = cmd.Parameters[$"@{nameof(Models.Person.MiddleInitial)}"].GetValue<string>(),
                    FamilyName = cmd.Parameters[$"@{nameof(Models.Person.FamilyName)}"].GetValue<string>(),
                };

                People.Add(person);

                person.ID = People.Count();
                person.FullName = String.Format("{0}, {1}{2}",
                    person.FamilyName, person.FirstName,
                    person.MiddleInitial.IsNotNullOrEmpty() ? $" {person.MiddleInitial}." : "");

                return new[] { person }.ToDataTable();
            });

            Context.Database.Instance.AddCommandBehavior(@"INSERT INTO [dbo].[Renter]
OUTPUT INSERTED.* INTO @output
VALUES
	(@PersonID, @UnitID, @Rent, @StartDate, @EndDate)", cmd =>
            {
                Models.Renter renter = new Models.Renter()
                {
                    PersonID = cmd.Parameters[$"@{nameof(Models.Renter.PersonID)}"].GetValue<int>(),
                    UnitID = cmd.Parameters[$"@{nameof(Models.Renter.UnitID)}"].GetValue<int>(),
                    Rent = cmd.Parameters[$"@{nameof(Models.Renter.Rent)}"].GetValue<decimal>(),
                    StartDate = cmd.Parameters[$"@{nameof(Models.Renter.StartDate)}"].GetValue<DateTime>(),
                    EndDate = cmd.Parameters[$"@{nameof(Models.Renter.EndDate)}"].GetValue<DateTime?>()
                };

                renter.PersonID.Should().BeGreaterThan(0);
                renter.UnitID.Should().BeGreaterThan(0);

                Renters.Add(renter);

                return new[] { renter }.ToDataTable();
            });
        }


        [Test]
        public void ShouldBeAbleToSaveAnObjectGraphToTheDatabase()
        {
            Models.Renter renter = new Models.Renter()
            {
                Person = GetFakePerson.Generate(),
                Unit = Context.Units.FindByID(4),
                Rent = 500M,
                StartDate = DateTime.Today
            };

            Context.Renters.Add(renter);

            if (Context.SaveChanges())
            {
                renter.Person.ID.Should().BeGreaterThan(0);
                renter.PersonID.Should().Be(renter.Person.ID);
            }
        }
    }
}
