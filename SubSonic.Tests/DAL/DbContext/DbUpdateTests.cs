using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;
    using SubSonic.Data.Caching;
    using Models = Extensions.Test.Models;

    public partial class DbContextTests
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
        }

        [Test]
        [TestCaseSource(nameof(UpdateTestCases))]
        public void CanUpdateEntities(IDbTestCase dbTest)
        {
            IEnumerable<IEntityProxy>
                expected = dbTest.FetchAll().Select(x =>
                    x as IEntityProxy);

            expected.Count().Should().Be(dbTest.Count());

            DbContext.Database.Instance.AddCommandBehavior(dbTest.Expectation, cmd =>
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

            foreach(IEntityProxy proxy in expected)
            {
                if (proxy is Models.Person person)
                {   // we should really set this part up to set random data
                    person.FirstName = "Bob";
                    person.FamilyName = "Walters";
                    person.MiddleInitial = "S";
                }

                proxy.IsDirty.Should().BeTrue();
            }

            DbContext.ChangeTracking
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
                        DbContext.SaveChanges().Should().BeTrue();
                    }
                }
                else
                {
                    DbContext.SaveChanges().Should().BeTrue();
                }

                FluentActions.Invoking(() =>
                    DbContext.Database.Instance.RecievedCommand(dbTest.Expectation))
                    .Should().NotThrow();

                DbContext.Database.Instance.RecievedCommandCount(dbTest.Expectation)
                    .Should()
                    .Be(dbTest.UseDefinedTableType ? 1 : expected.Count());

                foreach (IEntityProxy proxy in expected)
                {
                    if (proxy is Models.Person person)
                    {
                        person.FullName.Should().Be("Walters, Bob S.");
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

                        person.FirstName = (string)entity[nameof(Models.Person.FirstName)];
                        person.MiddleInitial = (string)entity[nameof(Models.Person.MiddleInitial)];
                        person.FamilyName = (string)entity[nameof(Models.Person.FamilyName)];

                        person.FullName = String.Format("{0}, {1}{2}",
                            person.FamilyName, person.FirstName,
                            person.MiddleInitial.IsNotNullOrEmpty() ? $" {person.MiddleInitial}." : "");

                        result.Add(person);
                    }

                    return result.ToDataTable();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
