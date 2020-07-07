using NUnit.Framework;
using SubSonic.Extensions.Test.Models;
using SubSonic.Extensions.Test;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DAL
{
    using FluentAssertions;
    using Linq;
    using System.Threading;

    [TestFixture]
    public class AsynchronousTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            string
                people_equal = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] == @id_1)",
                people_greater_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] > @id_1)",
                people_less_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] > @id_1)";

            Context.Database.Instance.AddCommandBehavior(people_greater_than, cmd => People.Where(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_equal, cmd => People.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_less_than, cmd => People.Where(x => x.ID < cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
        }

        [Test]
        public async Task ShouldBeAbleToGetSingleAsync()
        {
            Person person  = await Context.People.Where(x => x.ID == 1)
                .AsAsyncSubSonicQueryable()
                .SingleAsync();

            person.ID.Should().Be(1);
        }

        [Test]
        public void ShouldBeAbleToThrowWhenSingleAsyncHasMoreThanOne()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID > 0)
                  .AsAsyncSubSonicQueryable()
                  .SingleAsync();
            }).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShouldBeAbleToThrowSingleAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID == -1)
                .AsAsyncSubSonicQueryable()
                .SingleAsync();
            }).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ShouldBeAbleToGetSingleOrDefaultAsync()
        {
            Person person = await Context.People.Where(x => x.ID == 1)
                .AsAsyncSubSonicQueryable()
                .SingleOrDefaultAsync();

            person.ID.Should().Be(1);
        }

        [Test]
        public void ShouldBeAbleToNotThrowSingleOrDefaultAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                Person person = await Context.People.Where(x => x.ID == -1)
                .AsAsyncSubSonicQueryable()
                .SingleOrDefaultAsync();

                person.Should().BeNull();

            }).Should().NotThrow();
        }

        [Test]
        public async Task ShouldBeAbleToGetFirstAsync(CancellationToken cancellationToken)
        {
            Person person = await Context.People.Where(x => x.ID > 0)
                .AsAsyncSubSonicQueryable()
                .FirstAsync(cancellationToken);

            person.ID.Should().Be(1);
        }

        [Test]
        public void ShouldBeAbleToThrowFirstAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID < 1)
                .AsAsyncSubSonicQueryable()
                .FirstAsync();
            }).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ShouldBeAbleToGetFirstOrDefaultAsync()
        {
            Person person = await Context.People.Where(x => x.ID > 2)
                .AsAsyncSubSonicQueryable()
                .FirstOrDefaultAsync();

            person.ID.Should().Be(3);
        }

        [Test]
        public void ShouldBeAbleToNotThrowFirstOrDefaultAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                Person person = await Context.People.Where(x => x.ID < 1)
                .AsAsyncSubSonicQueryable()
                .SingleAsync();

                person.Should().BeNull();

            }).Should().NotThrow();
        }

        [Test]
        public async Task ShouldBeAbleToLoadResultSet()
        {
            FluentActions.Invoking(async () =>
            {
                ISubSonicCollection<Person> people = await Context.People
                .AsAsyncSubSonicQueryable()
                .LoadAsync();

                await foreach(Person person in people)
                {
                    person.FullName.Should().Be(String.Format("{0}, {1}{2}",
                        person.FamilyName, person.FirstName,
                        string.IsNullOrEmpty(person.MiddleInitial?.Trim()) ? "" : $" {person.MiddleInitial}."));
                }

            }).Should().NotThrow();
        }
    }
}
