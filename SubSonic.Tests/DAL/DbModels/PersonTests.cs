using Bogus;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace SubSonic.Tests.DAL.DbModels
{
    using Infrastructure.Schema;
    using Infrastructure;
    using Extensions.Test;
    using Models = Extensions.Test.Models;
    using System.Data.Common;
    using System.Data;

    [TestFixture]
    public class PersonTests
        : SUT.BaseTestFixture
    {
        [Test]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        public void CanInsertPersonRecords(int count)
        {
            Faker<Models.Person> people = new Faker<Models.Person>()
                .RuleFor(p => p.ID, f => 0)
                .RuleFor(p => p.FamilyName, x => x.PickRandom(DataSeed.Person.FamilyNames))
                .RuleFor(p => p.FirstName, x => x.PickRandom(DataSeed.Person.FirstNames))
                .RuleFor(p => p.MiddleInitial, x => x.PickRandom(DataSeed.Person.MiddleInitial))
                .RuleFor(p => p.Renters, x => new HashSet<Models.Renter>())
                .RuleFor(p => p.FullName, x => string.Empty);

            Context.People.AddRange(people.Generate(count));

            using (Context.Model.GetEntityModel<Models.Person>().AlteredState<IDbEntityModel, DbEntityModel>(new
            {
                DefinedTableType = new DbUserDefinedTableTypeAttribute(nameof(Models.Person))
            }).Apply())
            {
                string insert =
@"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO @output
SELECT
	[FirstName],
	[MiddleInitial],
	[FamilyName]
FROM @input";

                Context.Database.Instance.AddCommandBehavior(insert, cmd =>
                {
                    return InsertPersonRecords(cmd);
                });

                Context.SaveChanges().Should().BeTrue();
            }
        }

        private DataTable InsertPersonRecords(DbCommand cmd)
        {
            using (DataTable inserted = cmd.Parameters["@input"].GetValue<DataTable>())
            {
                ICollection<Models.Person> people = new List<Models.Person>();

                inserted.CreateDataReader().ReadData<Models.Person>((entity) =>
                {
                    People.Add(entity);

                    entity.ID = People.Count();
                    entity.FullName = String.Format("{0}, {1}{2}",
                       entity.FamilyName, entity.FirstName,
                       entity.MiddleInitial.IsNotNullOrEmpty() ? $" {entity.MiddleInitial}." : "");

                    people.Add(entity);
                });

                return people.ToDataTable();
            }
        }
    }
}
