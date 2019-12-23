using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.DbContextTests
{
    [TestFixture]
    public partial class DbContextTests
    {
        [Test]
        public void RealEstatePropertyModelHasRelationships()
        {
            IDbEntityModel model = DbContext.Model.GetEntityModel<RealEstateProperty>();

            model.HasRelationships.Should().BeTrue();
        }
    }
}
