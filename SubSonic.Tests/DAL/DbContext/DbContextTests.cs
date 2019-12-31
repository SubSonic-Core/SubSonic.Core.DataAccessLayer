using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL
{
    using FluentAssertions;
    using SUT;
    using System.Data.Common;

    [TestFixture]
    public partial class DbContextTests
        : BaseTestFixture
    {
        [Test]
        public void DbSetCollectionsShouldBeInitialized()
        {
            DbContext.RealEstateProperties.Should().NotBeNull();
            DbContext.Statuses.Should().NotBeNull();
            DbContext.Units.Should().NotBeNull();
        }

        [Test]
        public void DbOptionsShouldBeInitialized()
        {
            DbContext.Options.Should().NotBeNull();
        }

        [Test]
        public void DbModelShouldBeInitialized()
        {
            DbContext.Model.Should().NotBeNull();
        }

        [Test]
        public void ShouldBeAbleToCreateConnection()
        {
            DbConnection dbConnection = DbContext.Database.CreateConnection();

            dbConnection.Should().NotBeNull();
            dbConnection.ConnectionString.Should().NotBeNullOrEmpty();
        }
    }
}
