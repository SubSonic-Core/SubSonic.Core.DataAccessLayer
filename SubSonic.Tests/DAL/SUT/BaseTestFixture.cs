using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

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

        protected Bogus.Faker<Person> GetFakePerson
        {
            get
            {
                return new Bogus.Faker<Person>()
                    .RuleFor(person => person.ID, set => 0)
                    .RuleFor(person => person.FirstName, set => set.PickRandom(DataSeed.Person.FirstNames))
                    .RuleFor(person => person.MiddleInitial, set => set.PickRandom(DataSeed.Person.MiddleInitial))
                    .RuleFor(person => person.FamilyName, set => set.PickRandom(DataSeed.Person.FamilyNames))
                    .RuleFor(person => person.FullName, set => null)
                    .RuleFor(person => person.Renters, set => new HashSet<Renter>());
            }
        }

        [SetUp]
        public virtual void SetupTestFixture()
        {
            Context.ChangeTracking.Flush();

            Context.Instance.GetService<DbProviderFactory, SubSonicMockDbClient>().ClearBehaviors();

            Statuses = new List<Status>()
            {
                new Status() { ID = 1, Name = "Vacant", IsAvailableStatus = true },
                new Status() { ID = 2, Name = "Renovation", IsAvailableStatus = true },
                new Status() { ID = 3, Name = "Occupied", IsAvailableStatus = false },
            };

            Units = new List<Unit>()
            {
                new Unit() { ID = 1, RealEstatePropertyID = 1, StatusID = 1, NumberOfBedrooms = 1 },
                new Unit() { ID = 2, RealEstatePropertyID = 1, StatusID = 2, NumberOfBedrooms = 2 },
                new Unit() { ID = 3, RealEstatePropertyID = 1, StatusID = 3, NumberOfBedrooms = 3 },
                new Unit() { ID = 4, RealEstatePropertyID = 2, StatusID = 3, NumberOfBedrooms = 2 },
            };

            Renters = new List<Renter>()
            {
                new Renter() { PersonID = 1, UnitID = 1, StartDate = new DateTime(1980, 01, 01), EndDate = new DateTime(1990, 02, 28) },
                new Renter() { PersonID = 2, UnitID = 1, StartDate = new DateTime(1990, 03, 01) },
                new Renter() { PersonID = 3, UnitID = 2, StartDate = new DateTime(1980, 03, 01), EndDate = new DateTime(2000, 01, 01) },
                new Renter() { PersonID = 1, UnitID = 3, StartDate = new DateTime(1990, 03, 01) },
                new Renter() { PersonID = 4, UnitID = 4, StartDate = new DateTime(2000, 01, 01) }
            };

            RealEstateProperties = new List<RealEstateProperty>()
            {
                new RealEstateProperty() { ID = 1, StatusID = 1, HasParallelPowerGeneration = true },
                new RealEstateProperty() { ID = 2, StatusID = 3, HasParallelPowerGeneration = false },
                new RealEstateProperty() { ID = 3, StatusID = 2, HasParallelPowerGeneration = true },
                new RealEstateProperty() { ID = 4, StatusID = 2, HasParallelPowerGeneration = false },
            };

            People = new List<Person>()
            { 
                new Person() { ID = 1, FirstName = "Kara", FamilyName = "Danvers", FullName = "Danvers, Kara" },
                new Person() { ID = 2, FirstName = "Clark", FamilyName = "Kent", FullName = "Kent, Clark" },
                new Person() { ID = 3, FirstName = "First", FamilyName = "Last", FullName = "Last, First" },
                new Person() { ID = 4, FirstName = "First", FamilyName = "Last", FullName = "Last, First" }
            };
        }

        protected IEnumerable<Status> Statuses { get; set; }
        protected ICollection<Unit> Units { get; set; }
        protected ICollection<Renter> Renters { get; set; }
        protected ICollection<RealEstateProperty> RealEstateProperties { get; set; }
        protected ICollection<Person> People { get; set; }

        
    }
}
