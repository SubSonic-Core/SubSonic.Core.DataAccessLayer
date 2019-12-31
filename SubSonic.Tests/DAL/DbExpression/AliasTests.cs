using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.DbExpression
{
    using FluentAssertions;
    using Linq.Expressions.Alias;
    [TestFixture]
    public class AliasTests
    {
        TableAliasCollection aliases;

        [SetUp]
        public void SetUp()
        {
            aliases = new TableAliasCollection();
        }

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
        public void TableAliasCollectionCanBeReset()
        {
            TableAlias
                first = new TableAlias("RealEstateProperty"),
                second = new TableAlias("Unit"),
                third = new TableAlias("Status");

            string alias = aliases.NextAlias;

            aliases.GetAliasName(first).Should().Be(alias);
            aliases.Reset();
            aliases.GetAliasName(second).Should().Be(alias);
            aliases.Reset();
            aliases.GetAliasName(third).Should().Be(alias);
        }

        [Test]
        public void aliasesCanTrackAssignedAlias()
        {
            TableAlias
                first = new TableAlias("RealEstateProperty"),
                second = new TableAlias("RealEstateProperty"),
                third = new TableAlias("Status");

            string
                aliasOne = aliases.GetAliasName(first),
                aliasTwo = aliases.GetAliasName(second),
                aliasThree = aliases.GetAliasName(third);

            aliasOne.Should().Be("T1");
            aliasOne.Should().Be("T1");
            aliasOne.Should().Be("T1");

            aliasTwo.Should().Be("T2");
            aliasTwo.Should().Be("T2");
            aliasTwo.Should().Be("T2");

            aliasThree.Should().Be("T3");
            aliasThree.Should().Be("T3");
            aliasThree.Should().Be("T3");

            aliases.GetAliasName(first).Should().NotBe(aliasTwo);
            aliases.GetAliasName(first).Should().NotBe(aliasThree);
            aliases.GetAliasName(first).Should().Be(aliasOne);

            aliases.GetAliasName(second).Should().Be(aliasTwo);
            aliases.GetAliasName(second).Should().NotBe(aliasThree);
            aliases.GetAliasName(second).Should().NotBe(aliasOne);

            aliases.GetAliasName(third).Should().NotBe(aliasTwo);
            aliases.GetAliasName(third).Should().Be(aliasThree);
            aliases.GetAliasName(third).Should().NotBe(aliasOne);
        }
    }
}

