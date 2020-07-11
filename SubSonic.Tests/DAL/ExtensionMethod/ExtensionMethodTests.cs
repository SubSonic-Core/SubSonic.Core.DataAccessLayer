using FluentAssertions;
using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.ExtensionMethod
{
    [TestFixture]
    public class ExtensionMethodTests
        : BaseTestFixture
    {
        [Test]
        public void TheCountMethodIsSupported()
        {
            Context.People.Count().Should().BeGreaterThan(0).And.IsOfType<int>();
        }

        [Test]
        public void TheLongCountMethodIsSupported()
        {
            Context.People.LongCount().Should().BeGreaterThan(0).And.IsOfType<long>();
        }
    }
}
