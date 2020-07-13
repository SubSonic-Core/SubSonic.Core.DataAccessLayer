using Dasync.Collections;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.SUT
{
    public class BaseTestFixture
    {
        protected TestDbContext Context { get => SetUpSubSonic.DbContext; }

        protected ISubSonicLogger Logger { get; set; }

        public class DataSeed
        {
            public class Person
            {
                public static string[] FamilyNames => new[] { "Carter", "Scully", "Mulder", "Barker", "Ward", "Fury", "Danvers", "Williamson", "Walters", "Spiner", "Smith", "Picard", "Davis", "Trump", "Paris" };
                public static string[] FirstNames => new[] { "Abigail", "Alexander", "Ethan", "Evie", "Owen", "Bob", "Bethany", "Kara", "Clark", "Nick", "Kenneth", "Peggy", "Edward", "Brent", "William", "Jean" };
                public static string[] MiddleInitial => new[] { "", " ", "A", null, "B", "C", "D", "E", null, "F", "G", "H", "I", "J", "K", null, "L", "M", "N", "O", null, "P", null, "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", null };
            }
        }

        protected int personId;

        protected int propertyId;

        protected Bogus.Faker<Person> GetFakePerson
        {
            get
            {
                return new SubSonicFaker<Person>()
                    .UseDbContext()
                    .RuleFor(person => person.ID, set => ++personId)
                    .RuleFor(person => person.FirstName, set => set.PickRandom(DataSeed.Person.FirstNames))
                    .RuleFor(person => person.MiddleInitial, set => set.PickRandom(DataSeed.Person.MiddleInitial))
                    .RuleFor(person => person.FamilyName, set => set.PickRandom(DataSeed.Person.FamilyNames))
                    .RuleFor(person => person.FullName, set => null)
                    .FinishWith((faker, person) =>
                    {
                        if (person is IEntityProxy proxy)
                        {
                            proxy.IsDirty = false;
                        }
                    });
            }
        }

        protected Bogus.Faker<RealEstateProperty> GetFakeProperty
        {
            get => new SubSonicFaker<RealEstateProperty>()
                    .UseDbContext()
                    .RuleFor(property => property.ID, set => ++propertyId)
                    .RuleFor(property => property.StatusID, set => set.PickRandom(Statuses.Select(x => x.ID)))
                    .RuleFor(property => property.HasParallelPowerGeneration, set => set.PickRandom(true, false))
                    .FinishWith((faker, property) =>
                    {
                        if (property is IEntityProxy proxy)
                        {
                            proxy.IsDirty = false;
                        }
                    });
        }

        [SetUp]
        public virtual void SetupTestFixture()
        {
            Context.ChangeTracking.Flush();

            Context.Instance.GetService<DbProviderFactory, SubSonicMockDbClient>().ClearBehaviors();

            SetInsertBehaviors();

            SetSelectBehaviors();

            Statuses = new List<Status>()
            {
                new Status() { ID = 1, Name = "Vacant", IsAvailableStatus = true },
                new Status() { ID = 2, Name = "Renovation", IsAvailableStatus = true },
                new Status() { ID = 3, Name = "Occupied", IsAvailableStatus = false },
            };

            People = new List<Person>(GetFakePerson.Generate(50));

            RealEstateProperties = new List<RealEstateProperty>(GetFakeProperty.Generate(10));

            Units = new List<Unit>()
            {
                new Unit() { ID = 1, RealEstatePropertyID = 1, StatusID = 1, NumberOfBedrooms = 1 },
                new Unit() { ID = 2, RealEstatePropertyID = 1, StatusID = 2, NumberOfBedrooms = 2 },
                new Unit() { ID = 3, RealEstatePropertyID = 1, StatusID = 3, NumberOfBedrooms = 3 },
                new Unit() { ID = 4, RealEstatePropertyID = 2, StatusID = 3, NumberOfBedrooms = 2 },
            };

            Renters = new List<Renter>()
            {
                new Renter() { PersonID = 1, UnitID = 1, Rent = 100M, StartDate = new DateTime(1980, 01, 01), EndDate = new DateTime(1990, 02, 28) },
                new Renter() { PersonID = 2, UnitID = 1, Rent = 150M, StartDate = new DateTime(1990, 03, 01) },
                new Renter() { PersonID = 3, UnitID = 2, Rent = 200M, StartDate = new DateTime(1980, 03, 01), EndDate = new DateTime(2000, 01, 01) },
                new Renter() { PersonID = 1, UnitID = 3, Rent = 250M, StartDate = new DateTime(1990, 03, 01) },
                new Renter() { PersonID = 4, UnitID = 4, Rent = 300M, StartDate = new DateTime(2000, 01, 01) }
            };

        }

        [TearDown]
        public virtual void TearDownTestFixture()
        {
            personId = 0;
            propertyId = 0;
        }

        protected virtual void SetSelectBehaviors()
        {
            string
                people_all = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]",
                people_all_count = @"SELECT COUNT([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                people_all_long_count = @"SELECT COUNT_BIG([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                people_equal = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] = @id_1)",
                people_greater_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] > @id_1)",
                people_less_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] < @id_1)",
                renter_byperson = @"SELECT [T1].[PersonID], [T1].[UnitID], [T1].[Rent], [T1].[StartDate], [T1].[EndDate]
FROM [dbo].[Renter] AS [T1]
WHERE ([T1].[PersonID] == @personid_1)";

            Context.Database.Instance.AddCommandBehavior(people_all, cmd => BuildDataTable(People));
            Context.Database.Instance.AddCommandBehavior(people_all_count, cmd => People.Count());
            Context.Database.Instance.AddCommandBehavior(people_all_long_count, cmd => People.LongCount());
            Context.Database.Instance.AddCommandBehavior(people_greater_than, cmd => BuildDataTable(People.Where(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>())));
            Context.Database.Instance.AddCommandBehavior(people_equal, cmd => BuildDataTable(People.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>())));
            Context.Database.Instance.AddCommandBehavior(people_less_than, cmd => BuildDataTable(People.Where(x => x.ID < cmd.Parameters["@id_1"].GetValue<int>())));

            Context.Database.Instance.AddCommandBehavior(renter_byperson, cmd => BuildDataTable(Renters.Where(x => x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>())));
        }

        protected DataTable BuildDataTable<TEntity>(IEnumerable<TEntity> entities)
        {
            if (typeof(TEntity) == typeof(Person))
            {
                //foreach (TEntity entity in entities)
                Parallel.ForEach(entities, entity =>
                {
                    if (entity is Person person)
                    {
                        person.FullName = String.Format("{0}, {1}{2}",
                            person.FamilyName, person.FirstName,
                            string.IsNullOrEmpty(person.MiddleInitial?.Trim()) ? "" : $" {person.MiddleInitial}.");
                    }
                }
                );
            }

            return entities.ToDataTable();
        }

        protected virtual void SetInsertBehaviors()
        {
            string 
                insert_person = @"INSERT INTO [dbo].[Person]
OUTPUT INSERTED.* INTO @output
VALUES
	(@FirstName, @MiddleInitial, @FamilyName)";

            Context.Database.Instance.AddCommandBehavior(insert_person, cmd =>
            {
                Person data = new Person()
                {
                    FamilyName = cmd.Parameters["@FamilyName"].Value.ToString(),
                    FirstName = cmd.Parameters["@FirstName"].Value.ToString(),
                    MiddleInitial = cmd.Parameters["@MiddleInitial"].Value.IsNotNull(x => x.ToString())
                };

                People.Add(data);

                data.ID = People.Count;

                data.FullName = String.Format("{0}, {1}{2}",
                    data.FamilyName, data.FirstName,
                    string.IsNullOrEmpty(data.MiddleInitial?.Trim()) ? "" : $" {data.MiddleInitial}.");

                return new[] { data }.ToDataTable();
            });
        }

        protected IEnumerable<Status> Statuses { get; set; }
        protected ICollection<Unit> Units { get; set; }
        protected ICollection<Renter> Renters { get; set; }
        protected ICollection<RealEstateProperty> RealEstateProperties { get; set; }
        protected ICollection<Person> People { get; set; }

        
    }
}
