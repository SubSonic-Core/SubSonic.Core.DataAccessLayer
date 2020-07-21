using Dasync.Collections;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.Data.Builders;
using SubSonic.Extensions.Test.Models;
using SubSonic.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL.SUT
{
    public class BaseTestFixture
    {
        protected TestSubSonicContext Context
        {
            get
            {
                if (SetUpSubSonic.DbContext is null)
                {
                    SetUpSubSonic.SetDbContext();
                }
                return SetUpSubSonic.DbContext;
            }
        }

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

        protected int unitId;

        protected int renterId;

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

        protected Bogus.Faker<Unit> GetFakeUnit
        {
            get => new SubSonicFaker<Unit>()
                .UseDbContext()
                .RuleFor(unit => unit.ID, set => ++unitId)
                .RuleFor(unit => unit.RealEstatePropertyID, set => set.PickRandom(RealEstateProperties.Select(x => x.ID)))
                .RuleFor(unit => unit.NumberOfBedrooms, set => set.PickRandom(1, 2, 3, 4, 5, 6))
                .RuleFor(unit => unit.StatusID, set => set.PickRandom(Statuses.Select(x => x.ID)))
                .FinishWith((faker, unit) =>
                {
                    if (unit is IEntityProxy proxy)
                    {
                        proxy.IsDirty = false;
                    }
                });
        }

        protected Bogus.Faker<Renter> GetFakeRenter
        {
            get => new SubSonicFaker<Renter>()
                .UseDbContext()
                .RuleFor(renter => renter.ID, set => ++renterId)
                .RuleFor(renter => renter.PersonID, set => set.PickRandom(People.Select(x => x.ID)))
                .RuleFor(renter => renter.UnitID, set => set.PickRandom(Units.Select(x => x.ID)))
                .RuleFor(renter => renter.Rent, set => set.PickRandom(100M, 200M, 300M, 400M, 500M, 600M))
                .RuleFor(renter => renter.StartDate, set => DateTime.Today)
                .FinishWith((faker, renter) =>
                {
                    if (renter is IEntityProxy proxy)
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

            SetDeleteBehaviors();

            Statuses = new List<Status>()
            {
                new Status() { ID = 1, Name = "Vacant", IsAvailableStatus = true },
                new Status() { ID = 2, Name = "Renovation", IsAvailableStatus = true },
                new Status() { ID = 3, Name = "Occupied", IsAvailableStatus = false },
            };

            People = new List<Person>(GetFakePerson.Generate(50));

            RealEstateProperties = new List<RealEstateProperty>(GetFakeProperty.Generate(10));

            Units = new List<Unit>(GetFakeUnit.Generate(25));

            Renters = new List<Renter>(GetFakeRenter.Generate(50));

        }

        [TearDown]
        public virtual void TearDownTestFixture()
        {
            personId = 0;
            propertyId = 0;
            unitId = 0;
            renterId = 0;
        }

        protected virtual void SetDeleteBehaviors()
        {

        }

        protected virtual void SetSelectBehaviors()
        {
            string
                people_all = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]",
                people_all_count = @"SELECT COUNT(*)
FROM [dbo].[Person] AS [T1]",
                people_all_long_count = @"SELECT COUNT_BIG(*)
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
                renter_byperson = @"SELECT [T1].[ID], [T1].[PersonID], [T1].[UnitID], [T1].[Rent], [T1].[StartDate], [T1].[EndDate]
FROM [dbo].[Renter] AS [T1]
WHERE ([T1].[PersonID] = @personid_1)",
                people_page_count_all = @"SELECT COUNT([T1].[ID]) [RECORDCOUNT]
FROM [dbo].[Person] AS [T1]",
                people_paged =
@"WITH page AS
(
	SELECT [T1].[ID]
	FROM [dbo].[Person] AS [T1]
	ORDER BY [T1].[ID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
	INNER JOIN page
		ON ([page].[ID] = [T1].[ID])
OPTION (RECOMPILE)",
                renters_all = @"SELECT [T1].[ID], [T1].[PersonID], [T1].[UnitID], [T1].[Rent], [T1].[StartDate], [T1].[EndDate]
FROM [dbo].[Renter] AS [T1]",
                renters_all_cnt = @"SELECT COUNT(*)
FROM [dbo].[Renter] AS [T1]",
                memberInitializedUsingDatasource = @"SELECT [T1].[PersonID], [T2].[FullName], [T1].[Rent], [T1].[UnitID], COALESCE([T3].[HasParallelPowerGeneration], 0) AS [HasParallelPowerGeneration], [T4].[Name] AS [Status]
FROM [dbo].[Renter] AS [T1]
	INNER JOIN [dbo].[Unit] AS [T5]
		ON ([T5].[ID] = [T1].[UnitID])
	INNER JOIN [dbo].[RealEstateProperty] AS [T3]
		ON ([T3].[ID] = [T5].[RealEstatePropertyID])
	INNER JOIN [dbo].[Status] AS [T4]
		ON ([T4].[ID] = [T3].[StatusID])
	INNER JOIN [dbo].[Person] AS [T2]
		ON ([T2].[ID] = [T1].[PersonID])
WHERE @dt_value_1 BETWEEN [T1].[StartDate] AND COALESCE([T1].[EndDate], @dt_value_2)",
                memberInitializedUsingDatasourceCount = @"SELECT COUNT(*)
FROM [dbo].[Renter] AS [T1]
	INNER JOIN [dbo].[Unit] AS [T2]
		ON ([T2].[ID] = [T1].[UnitID])
	INNER JOIN [dbo].[RealEstateProperty] AS [T3]
		ON ([T3].[ID] = [T2].[RealEstatePropertyID])
	INNER JOIN [dbo].[Status] AS [T4]
		ON ([T4].[ID] = [T3].[StatusID])
	INNER JOIN [dbo].[Person] AS [T5]
		ON ([T5].[ID] = [T1].[PersonID])
WHERE @dt_value_1 BETWEEN [T1].[StartDate] AND COALESCE([T1].[EndDate], @dt_value_2)";

            Context.Database.Instance.AddCommandBehavior(people_all, cmd =>
                BuildDataTable(People));
            Context.Database.Instance.AddCommandBehavior(people_all_count, cmd =>
                People.Count());
            Context.Database.Instance.AddCommandBehavior(people_all_long_count, cmd =>
                People.LongCount());
            Context.Database.Instance.AddCommandBehavior(people_greater_than, cmd =>
                BuildDataTable(People.Where(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>())));
            Context.Database.Instance.AddCommandBehavior(people_equal, cmd =>
                BuildDataTable(People.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>())));
            Context.Database.Instance.AddCommandBehavior(people_less_than, cmd =>
                BuildDataTable(People.Where(x => x.ID < cmd.Parameters["@id_1"].GetValue<int>())));

            Context.Database.Instance.AddCommandBehavior(renter_byperson, cmd =>
                BuildDataTable(Renters.Where(x => x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>())));

            Context.Database.Instance.AddCommandBehavior(people_page_count_all, (cmd) =>
            {
                using (DataTableBuilder table = new DataTableBuilder())
                {
                    table
                    .AddColumn("RECORDCOUNT", typeof(int))
                    .AddRow(People.Count());

                    return table.DataTable;
                }
            });
            Context.Database.Instance.AddCommandBehavior(people_paged, (cmd) =>
            {
                int size = 0, page = 0;

                foreach (DbParameter parameter in cmd.Parameters)
                {
                    if (parameter.ParameterName.Contains("PageSize"))
                    {
                        size = (int)parameter.Value;
                    }
                    else if (parameter.ParameterName.Contains("PageNumber"))
                    {
                        page = (int)parameter.Value;
                    }
                }

                return People
                    .Skip(size * (page - 1))
                    .Take(size)
                    .ToDataTable();
            });

            Context.Database.Instance.AddCommandBehavior(renters_all, cmd =>
                BuildDataTable(Renters));
            Context.Database.Instance.AddCommandBehavior(renters_all_cnt, cmd =>
                Renters.Count());

            Context.Database.Instance.AddCommandBehavior(memberInitializedUsingDatasource, cmd =>
            {
                return BuildDataTable(CurrentRenters.ToList());
            });

            Context.Database.Instance.AddCommandBehavior(memberInitializedUsingDatasourceCount, cmd =>
            {
                return CurrentRenters.Count();
            });
        }

        protected IEnumerable<RenterView> CurrentRenters => Renters
                    .Join(People, x => x.PersonID, x => x.ID, (r, p) =>
                    {
                        r.Person = p;

                        return r;
                    })
                    .Join(Units.Join(RealEstateProperties.Join(Statuses, rep => rep.StatusID, s => s.ID, (rep, s) =>
                    {
                        rep.Status = s;
                        return rep;
                    }), u => u.RealEstatePropertyID, rep => rep.ID, (u, rep) =>
                                      {
                                          u.RealEstateProperty = rep;
                                          return u;
                                      }), r => r.UnitID, u => u.ID, (r, u) =>
                                      {
                                          r.Unit = u;
                                          return r;
                                      })
            .Where(r => DateTime.Today >= r.StartDate && DateTime.Today <= r.EndDate.GetValueOrDefault(DateTime.Today.AddDays(1)))
            .Select(x => new RenterView()
            {
                FullName = x.Person.FullName,
                HasParallelPowerGeneration = x.Unit.RealEstateProperty.HasParallelPowerGeneration.GetValueOrDefault(),
                PersonID = x.PersonID,
                UnitID = x.UnitID,
                Status = x.Unit.RealEstateProperty.Status.Name,
                Rent = x.Rent
            });

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
            else if (typeof(TEntity) == typeof(RenterView))
            {
                Parallel.ForEach(entities, entity =>
                {
                    if (entity is RenterView renterView)
                    {
                        var person = People.Single(x => x.ID == renterView.PersonID);

                        renterView.FullName = String.Format("{0}, {1}{2}",
                            person.FamilyName, person.FirstName,
                            string.IsNullOrEmpty(person.MiddleInitial?.Trim()) ? "" : $" {person.MiddleInitial}.");
                    }
                }
                );
            }

            DataTable data = entities.ToDataTable();

            return data;
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
