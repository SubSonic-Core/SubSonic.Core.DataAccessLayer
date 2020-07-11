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
    using Extensions.Test;

    [TestFixture]
    public class ExtensionMethodTests
        : BaseTestFixture
    {
        protected override void SetSelectBehaviors()
        {
            base.SetSelectBehaviors();

            string
                person_min = @"SELECT MIN([T1].[ID])
FROM [dbo].[Person] AS [T1]";

            Context.Database.Instance.AddCommandBehavior(person_min, cmd => People.Min(x => x.ID));
        }


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

        [Test]
        public void TheMinMethodIsSupported()
        {
            Context.People.Min(x => x.ID).Should().Be(1);
        }
    }
}
