using NUnit.Framework;
using SubSonic.Extensions.Test.Models;
using SubSonic.Tests.DAL.SUT;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace SubSonic.Tests.DAL.Collections
{
    using Linq;
    using Extensions.Test;
    
    [TestFixture]
    public class CollectionTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            string
                people_greater_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] > @id_1)";

            Context.Database.Instance.AddCommandBehavior(people_greater_than.Format("T1"), cmd => People.Where(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
        }

        [Test]
        public void CollectionShouldLoadAndCastToArray()
        {
            Person[] people = Context.People.Where(x => x.ID > 2).ToArray();

            people.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public void CollectionShouldLoadAndCastToList()
        {
            List<Person> people = Context.People.Where(x => x.ID > 2).ToList();

            people.Count.Should().BeGreaterThan(0);
        }
    }
}
