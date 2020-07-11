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
    using SubSonic.Extensions.Test.Models;

    [TestFixture]
    public class ExtensionMethodTests
        : BaseTestFixture
    {
        protected override void SetSelectBehaviors()
        {
            base.SetSelectBehaviors();

            string
                person_min = @"SELECT MIN([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                person_max = @"SELECT MAX([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                renter_sum = @"SELECT SUM([T1].[Rent])
FROM [dbo].[Renter] AS [T1]
WHERE ([T1].[PersonID] = @personid_1)",
                renter_avg = @"SELECT AVG([T1].[Rent])
FROM [dbo].[Renter] AS [T1]
WHERE ([T1].[PersonID] = @personid_1)";

            Context.Database.Instance.AddCommandBehavior(person_max, cmd => People.Max(x => x.ID));
            Context.Database.Instance.AddCommandBehavior(person_min, cmd => People.Min(x => x.ID));
            Context.Database.Instance.AddCommandBehavior(renter_sum, cmd => Renters.Where(x => x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>()).Sum(x => x.Rent));
            Context.Database.Instance.AddCommandBehavior(renter_avg, cmd => Renters.Where(x => x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>()).Average(x => x.Rent));
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

        [Test]
        public void TheMaxMethodIsSupported()
        {
            Context.People.Max(x => x.ID).Should().Be(People.Count);
        }

        [Test]
        public void TheSumMethodIsSupported()
        {
            Person person = Context.People.Single(x => x.ID == 1);

            person.Renters.Sum(x => x.Rent).Should().Be(Renters.Where(x => x.PersonID == person.ID).Sum(x => x.Rent));
        }

        [Test]
        public void TheAverageMethodIsSupported()
        {
            Person person = Context.People.Single(x => x.ID == 1);

            person.Renters.Average(x => x.Rent).Should().Be(Renters.Where(x => x.PersonID == person.ID).Average(x => x.Rent));
        }
    }
}
