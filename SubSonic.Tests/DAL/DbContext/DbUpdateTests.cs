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
        }
    }
}
