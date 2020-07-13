using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test.Models;
using SubSonic.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL
{
    [TestFixture]
    public partial class SubSonicContextTests
    {
        [Test]
        public void RealEstatePropertyModelHasRelationships()
        {
            IDbEntityModel model = Context.Model.GetEntityModel<RealEstateProperty>();

            model.HasRelationships.Should().BeTrue();
        }

        [Test]
        public void PersonPropertyFullNameShouldBeReadOnly()
        {
            IDbEntityModel model = Context.Model.GetEntityModel<Person>();

            model.Properties.First(x => x.Name == nameof(Person.FullName)).IsReadOnly.Should().BeTrue();
        }
    }
}
