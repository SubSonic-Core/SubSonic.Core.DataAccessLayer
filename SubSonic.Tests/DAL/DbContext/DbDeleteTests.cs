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
    using Models = Extensions.Test.Models;

    public partial class DbContextTests
    {
        const string expected_delete_udtt = @"DELETE FROM {0}
WHERE ([{1}].[ID] IN (SELECT [ID] FROM @input))";

        private static IEnumerable<IDbTestCase> DeleteTestCases()
        {
            yield return new DbTestCase<Models.Person>(false, @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (1)");
            yield return new DbTestCase<Models.Person>(true, @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (
	SELECT [T1].[ID]
	FROM @input AS [T1])");
        }

        [Test]
        [TestCaseSource(nameof(DeleteTestCases))]
        public void ShouldBeAbleToDeleteOneOrMoreRecords(IDbTestCase dbTest)
        {
            IEnumerable<IEntityProxy>
                expected = dbTest.FetchAll().Select(x =>
                    x as IEntityProxy);

            dbTest.Count().Should().Be(expected.Count());

            DbContext.Database.Instance.AddCommandBehavior(dbTest.Expectation, cmd =>
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

            DbContext.ChangeTracking
                .SelectMany(x => x.Value)
                .Count(x => x.IsDeleted)
                .Should().Be(expected.Count());

            if (expected.Count() > 0)
            {
                AlteredState<IDbEntityModel, DbEntityModel> state = null;

                if (dbTest.UseDefinedTableType)
                {
                    state = dbTest.EntityModel.AlteredState<IDbEntityModel, DbEntityModel>(new
                    {
                        DefinedTableType = new DbUserDefinedTableTypeAttribute(dbTest.EntityModel.Name)
                    }).Apply();
                }

                DbContext.SaveChanges().Should().BeTrue();

                if (dbTest.UseDefinedTableType)
                {
                    state.Dispose();
                    state = null;
                }

                dbTest.Count().Should().Be(0);
            }
        }

        private DataTable DeleteCmdBehaviorForUDTT(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            if (cmd.Parameters["@input"].Value is DataTable data)
            {
                using(data)
                {
                    foreach(DataRow row in data.Rows)
                    {
                        if (expected.Count() > 0 && expected.ElementAt(0) is Models.Person)
                        {
                            People.Remove(People.Single(x => x.ID == (int)row[nameof(Models.Person.ID)]));
                        }
                    }
                }
            }

            return null;
        }

        private DataTable DeleteCmdBehaviorForInArray(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            foreach (DbParameter parameter in cmd.Parameters)
            {
                if (parameter.Direction != ParameterDirection.Input)
                {
                    continue;
                }

                foreach (IEntityProxy proxy in expected)
                {
                    if (proxy is Models.Person person)
                    {
                        People.Remove(People.Single(x => x.ID == person.ID));
                    }
                }
            }

            return null;
        }
    }
}
