using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    
    using Schema;
    using Linq;
    using Models = Extensions.Test.Models;

    public partial class SubSonicContextTests
    {
        const string expected_delete_udtt = @"DELETE FROM {0}
WHERE ([{1}].[ID] IN (SELECT [ID] FROM @input))";

        private static IEnumerable<IDbTestCase> DeleteTestCases()
        {
            yield return new DbTestCase<Models.Person>(false, @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (@el_1)");
            yield return new DbTestCase<Models.Person>(true, @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (
	SELECT [T1].[ID]
	FROM @input AS [T1])");
            yield return new DbTestCase<Models.Renter>(false, @"DELETE FROM [dbo].[Renter]
WHERE ([PersonID] IN (@el_1) AND [UnitID] IN (@el_2))");
            yield return new DbTestCase<Models.Renter>(true, @"DELETE FROM [dbo].[Renter]
WHERE ([PersonID] IN (
	SELECT [T1].[PersonID]
	FROM @input AS [T1]) AND [UnitID] IN (
	SELECT [T2].[UnitID]
	FROM @input AS [T2]))");
        }

        [Test]
        [TestCaseSource(nameof(DeleteTestCases))]
        public void ShouldBeAbleToDeleteOneOrMoreRecords(IDbTestCase dbTest)
        {
            IEnumerable<IEntityProxy>
                expected = dbTest.FetchAll().Select(x =>
                    x as IEntityProxy);

            dbTest.Count().Should().Be(expected.Count());

            Context.Database.Instance.AddCommandBehavior(dbTest.Expectation, cmd =>
            {
                if (dbTest.UseDefinedTableType)
                {
                    return DeleteCmdBehaviorForUDTT(cmd, expected);
                }
                else
                {
                    return DeleteCmdBehaviorForInArray(cmd, expected);
                }
            });

            dbTest.Delete(expected);

            Context.ChangeTracking
                .SelectMany(x => x.Value)
                .Count(x => x.IsDeleted)
                .Should().Be(expected.Count());

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

                dbTest.Count().Should().Be(0);
            }
        }

        private int DeleteCmdBehaviorForUDTT(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            if (cmd.Parameters["@input"].Value is DataTable data)
            {
                using(data)
                {
                    foreach(DataRow row in data.Rows)
                    {
                        if (expected.Count() > 0)
                        {
                            if (expected.ElementAt(0) is Models.Person)
                            {
                                People.Remove(People.Single(x => x.ID == (int)row[nameof(Models.Person.ID)]));
                            }
                            else if (expected.ElementAt(0) is Models.Renter)
                            {
                                Renters.Remove(Renters.Single(x =>
                                   x.PersonID == (int)row[nameof(Models.Renter.PersonID)] &&
                                   x.UnitID == (int)row[nameof(Models.Renter.UnitID)] &&
                                   x.StartDate == (DateTime)row[nameof(Models.Renter.StartDate)]));
                            }
                        }
                    }
                }
            }

            return expected.Count();
        }

        private int DeleteCmdBehaviorForInArray(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            IEntityProxy proxy = expected.ElementAt(0);

            int count = 0;

            if (proxy is Models.Person)
            {
                People.Remove(People.Single(x => x.ID == cmd.Parameters["@el_1"].GetValue<int>()));

                count++;
            }
            else if (proxy is Models.Renter)
            {
                IEnumerable<Models.Renter> deleted = Renters.Where(x =>
                        x.PersonID == cmd.Parameters["@el_1"].GetValue<int>() &&
                        x.UnitID == cmd.Parameters["@el_2"].GetValue<int>())
                    .ToArray();

                foreach (Models.Renter renter in deleted)
                {
                    Renters.Remove(renter);

                    count++;
                }
            }

            return count;
        }
    }
}
