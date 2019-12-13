using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.DbExpressionTests
{
    using FluentAssertions;
    using Linq.Expressions.Alias;
    [TestFixture]
    public class AliasTests
    {
        [Test]
        public void TableAliasSupportsEquality()
        {
            TableAlias
                aliasOne = new TableAlias("RealEstateProperty"),
                aliasTwo = new TableAlias("RealEstateProperty");

            (aliasOne == aliasTwo).Should().BeFalse();
#pragma warning disable CS1718 // Comparison made to same variable
            (aliasOne == aliasOne).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable

            (aliasOne != aliasTwo).Should().BeTrue();

            aliasOne.Equals(aliasOne).Should().BeTrue();
        }

        [Test]
        public void TableAliasCollectionCanTrackAssignedAlias()
        {
            TableAlias
                first = new TableAlias("RealEstateProperty"),
                second = new TableAlias("RealEstateProperty"),
                third = new TableAlias("Status");

            string
                aliasOne = TableAliasCollection.GetAliasName(first),
                aliasTwo = TableAliasCollection.GetAliasName(second),
                aliasThree = TableAliasCollection.GetAliasName(third);

            aliasOne.Should().Be("T1");
            aliasOne.Should().Be("T1");
            aliasOne.Should().Be("T1");

            aliasTwo.Should().Be("T2");
            aliasTwo.Should().Be("T2");
            aliasTwo.Should().Be("T2");

            aliasThree.Should().Be("T3");
            aliasThree.Should().Be("T3");
            aliasThree.Should().Be("T3");

            TableAliasCollection.GetAliasName(first).Should().NotBe(aliasTwo);
            TableAliasCollection.GetAliasName(first).Should().NotBe(aliasThree);
            TableAliasCollection.GetAliasName(first).Should().Be(aliasOne);

            TableAliasCollection.GetAliasName(second).Should().Be(aliasTwo);
            TableAliasCollection.GetAliasName(second).Should().NotBe(aliasThree);
            TableAliasCollection.GetAliasName(second).Should().NotBe(aliasOne);

            TableAliasCollection.GetAliasName(third).Should().NotBe(aliasTwo);
            TableAliasCollection.GetAliasName(third).Should().Be(aliasThree);
            TableAliasCollection.GetAliasName(third).Should().NotBe(aliasOne);
        }
    }
}

