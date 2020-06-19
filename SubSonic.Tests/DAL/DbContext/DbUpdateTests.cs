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
        }
    }
}
