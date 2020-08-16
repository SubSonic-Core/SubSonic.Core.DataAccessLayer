using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using SubSonic.Extensions.Test;

namespace SubSonic.Tests.DAL
{
    using Schema;
    using Linq;
    using SubSonic.Data.Caching;
    using Models = Extensions.Test.Models;
    using SubSonic.Tests.DAL.SUT;

    public partial class SubSonicContextTests
    {
        private static IEnumerable<IDbTestCase> UpdateTestCases()
        {
            yield return new DbTestCase<Models.Person>(true, @"UPDATE [T1] SET
	[T1].[FirstName] = [T2].[FirstName],
	[T1].[MiddleInitial] = [T2].[MiddleInitial],
	[T1].[FamilyName] = [T2].[FamilyName]
OUTPUT INSERTED.* INTO @output
FROM [dbo].[Person] AS [T1]
	INNER JOIN @update AS [T2]
		ON ([T2].[ID] = [T1].[ID])", person => person.ID > 2);
            yield return new DbTestCase<Models.Person>(false, @"UPDATE [T1] SET
	[T1].[FirstName] = @FirstName,
	[T1].[MiddleInitial] = @MiddleInitial,
	[T1].[FamilyName] = @FamilyName
OUTPUT INSERTED.* INTO @output
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] = @id_1)", person => person.ID == 3);
            yield return new DbTestCase<Models.Renter>(true, @"UPDATE [T1] SET
	[T1].[PersonID] = [T2].[PersonID],
	[T1].[UnitID] = [T2].[UnitID],
	[T1].[Rent] = [T2].[Rent],
	[T1].[StartDate] = [T2].[StartDate],
	[T1].[EndDate] = [T2].[EndDate]
OUTPUT INSERTED.* INTO @output
FROM [dbo].[Renter] AS [T1]
	INNER JOIN @update AS [T2]
		ON (([T2].[PersonID] = [T1].[PersonID]) AND ([T2].[UnitID] = [T1].[UnitID]))", renter => renter.PersonID == 2 && renter.UnitID == 1);
            yield return new DbTestCase<Models.Renter>(false, @"UPDATE [T1] SET
	[T1].[PersonID] = @PersonID,
	[T1].[UnitID] = @UnitID,
	[T1].[Rent] = @Rent,
	[T1].[StartDate] = @StartDate,
	[T1].[EndDate] = @EndDate
OUTPUT INSERTED.* INTO @output
FROM [dbo].[Renter] AS [T1]
WHERE (([T1].[PersonID] = @personid_1) AND ([T1].[UnitID] = @unitid_2))", renter => renter.PersonID == 1 && renter.UnitID == 3);
        }

        [Test]
        [TestCaseSource(nameof(UpdateTestCases))]
        public void CanUpdateEntities(IDbTestCase dbTest)
        {
            IList<IEntityProxy>
                 expected = new List<IEntityProxy>();

            foreach (IEntityProxy proxy in dbTest.FetchAll())
            {
                if (proxy != null)
                {
                    expected.Add(proxy);
                }
            }

            expected.Count().Should().BeLessOrEqualTo(dbTest.Count());

            Context.Database.Instance.AddCommandBehavior(dbTest.Expectation, cmd =>
            {
                if (dbTest.UseDefinedTableType)
                {
                    return UpdateCmdBehaviorForUDTT(cmd, expected);
                }
                else
                {
                    return UpdateCmdBehaviorForInArray(cmd, expected);
                }
            });

            for(int i = 0, c = expected.Count(); i < c; i++)
            {
                IEntityProxy proxy = expected.ElementAt(i);

                if (proxy is Models.Person person)
                {   // we should really set this part up to set random data
                    person.FirstName = "Bob";
                    person.FamilyName = "Walters";
                    person.MiddleInitial = ((i % 2) == 0) ? "S" : null;
                }
                else if (proxy is Models.Renter renter)
                {
                    renter.EndDate.Should().NotBe(DateTime.Today);

                    renter.EndDate = DateTime.Today;
                }

                proxy.IsDirty.Should().BeTrue();
            }

            Context.ChangeTracking
                .SelectMany(x => x.Value)
                .Count(x => x.IsDirty)
                .Should()
                .Be(expected.Count());

            if (expected.Count() > 0)
            {
                if (dbTest.UseDefinedTableType)
                {
                    using (dbTest.EntityModel.AlteredState<IDbEntityModel, DbEntityModel>(new
                    {
                        DefinedTableType = new DbUserDefinedTableTypeAttribute(dbTest.EntityModel.Name)
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
                    Context.Database.Instance.RecievedCommand(dbTest.Expectation))
                    .Should().NotThrow();

                Context.Database.Instance.RecievedCommandCount(dbTest.Expectation)
                    .Should()
                    .Be(dbTest.UseDefinedTableType ? 1 : expected.Count());

                for (int i = 0, c = expected.Count(); i < c; i++)
                {
                    IEntityProxy proxy = expected.ElementAt(i);

                    if (proxy is Models.Person person)
                    {
                        if ((i % 2) == 0)
                        {
                            person.FullName.Should().Be("Walters, Bob S.");
                        }
                        else
                        {
                            person.FullName.Should().Be("Walters, Bob");
                        }
                    }
                    else if (proxy is Models.Renter renter)
                    {
                        renter.EndDate.Should().Be(DateTime.Today);
                    }

                    proxy.IsDirty.Should().BeFalse();
                }
            }
        }

        private DataTable UpdateCmdBehaviorForInArray(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            IEntityProxy proxy = expected.ElementAt(0);

            if (proxy is Models.Person)
            {
                Models.Person person = People.Single(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>());

                person.FirstName = cmd.Parameters["@FirstName"].GetValue<string>();
                person.MiddleInitial = cmd.Parameters["@MiddleInitial"].GetValue<string>();
                person.FamilyName = cmd.Parameters["@FamilyName"].GetValue<string>();

                person.FullName = String.Format("{0}, {1}{2}",
                    person.FamilyName, person.FirstName,
                    person.MiddleInitial.IsNotNullOrEmpty() ? $" {person.MiddleInitial}." : "");

                return new[] { person }.ToDataTable();
            }
            else if (proxy is Models.Renter)
            {
                Models.Renter renter = Renters.Single(x =>
                    x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>() &&
                    x.UnitID == cmd.Parameters["@unitid_2"].GetValue<int>());

                renter.PersonID = cmd.Parameters[$"@{nameof(Models.Renter.PersonID)}"].GetValue<int>();
                renter.UnitID = cmd.Parameters[$"@{nameof(Models.Renter.UnitID)}"].GetValue<int>();
                renter.Rent = cmd.Parameters[$"@{nameof(Models.Renter.Rent)}"].GetValue<decimal>();
                renter.StartDate = cmd.Parameters[$"@{nameof(Models.Renter.StartDate)}"].GetValue<DateTime>();
                renter.EndDate = cmd.Parameters[$"@{nameof(Models.Renter.EndDate)}"].GetValue<DateTime>();

                return new[] { renter }.ToDataTable();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private DataTable UpdateCmdBehaviorForUDTT(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            using (DataTable table = cmd.Parameters["@update"].GetValue<DataTable>())
            {
                if (expected.ElementAt(0) is Models.Person)
                {
                    List<Models.Person> result = new List<Models.Person>();

                    foreach (DataRow entity in table.Rows)
                    {
                        Models.Person person = People.Single(x => x.ID == (int)entity[nameof(Models.Person.ID)]);

                        person.FirstName = entity[nameof(Models.Person.FirstName)].GetValue<string>();
                        person.MiddleInitial = entity[nameof(Models.Person.MiddleInitial)].GetValue<string>();
                        person.FamilyName = entity[nameof(Models.Person.FamilyName)].GetValue<string>();

                        person.FullName = String.Format("{0}, {1}{2}",
                            person.FamilyName, person.FirstName,
                            person.MiddleInitial.IsNotNullOrEmpty() ? $" {person.MiddleInitial}." : "");

                        result.Add(person);
                    }

                    return result.OrderByDescending(x => x.ID).ToDataTable();
                }
                else if (expected.ElementAt(0) is Models.Renter)
                {
                    List<Models.Renter> result = new List<Models.Renter>();

                    foreach (DataRow entity in table.Rows)
                    {
                        Models.Renter renter = Renters.Single(x => 
                            x.PersonID == (int)entity[nameof(Models.Renter.PersonID)] &&
                            x.UnitID == (int)entity[nameof(Models.Renter.UnitID)]);

                        renter.PersonID = (int)entity[nameof(Models.Renter.PersonID)];
                        renter.UnitID = (int)entity[nameof(Models.Renter.UnitID)];
                        renter.Rent = (decimal)entity[nameof(Models.Renter.Rent)];
                        renter.StartDate = (DateTime)entity[nameof(Models.Renter.StartDate)];
                        renter.EndDate = (DateTime?)entity[nameof(Models.Renter.EndDate)];

                        result.Add(renter);
                    }

                    return result
                        .OrderByDescending(x => x.PersonID)
                        .ThenByDescending(x => x.UnitID)
                        .ToDataTable();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
