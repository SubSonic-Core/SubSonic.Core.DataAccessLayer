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
        private static IEnumerable<IDbTestCase> UpdateTestCases()
        {
            yield return new DbTestCase<Models.Person>(true, @"UPDATE [{0}] SET", person => person.ID > 2);
            yield return new DbTestCase<Models.Person>(false, @"UPDATE [{0}] SET", person => person.ID == 3);
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

            //if (expected.Count() > 0)
            //{
            //    if (dbTest.UseDefinedTableType)
            //    {
            //        using (dbTest.EntityModel.AlteredState<IDbEntityModel, DbEntityModel>(new
            //        {
            //            DefinedTableType = new DbUserDefinedTableTypeAttribute(dbTest.EntityModel.Name)
            //        }).Apply())
            //        {
            //            DbContext.SaveChanges().Should().BeTrue();
            //        }
            //    }
            //    else
            //    {
            //        DbContext.SaveChanges().Should().BeTrue();
            //    }

            //    FluentActions.Invoking(() =>
            //        DbContext.Database.Instance.RecievedCommand(dbTest.Expectation))
            //        .Should().NotThrow();

            //    DbContext.Database.Instance.RecievedCommandCount(dbTest.Expectation)
            //        .Should()
            //        .Be(dbTest.UseDefinedTableType ? 1 : expected.Count());
            //}
        }

        private object UpdateCmdBehaviorForInArray(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            throw new NotImplementedException();
        }

        private object UpdateCmdBehaviorForUDTT(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            throw new NotImplementedException();
        }
    }
}
