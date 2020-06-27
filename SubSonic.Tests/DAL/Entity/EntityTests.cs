using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.Entity
{
    using Data.Caching;
    using Extensions.Test.Models;
    using FluentAssertions;

    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void EntityNavigationPropertyWillSetForeignKeysOnSet()
        {
            var renter = new Renter();
            var entity = new Entity<Renter>(renter);

            renter.Person = new Person() { ID = 1 };
            renter.Unit = new Unit() { ID = 1 };

            entity.EnsureForeignKeys();

            renter.PersonID.Should().Be(1);
            renter.UnitID.Should().Be(1);
        }

        [Test]
        public void EntityShouldBeAbleToSetComputedFields()
        {
            Person  person_new = new Person(),
                    person_after = new Person() { ID = 1, FullName = "After" };

            Entity<Person> entity_new = new Entity<Person>(person_new),
                            entity_after = new Entity<Person>(person_after);

            entity_new.SetDbComputedProperties(entity_after);

            person_new.FullName.Should().Be(person_after.FullName);
        }
    }
}
