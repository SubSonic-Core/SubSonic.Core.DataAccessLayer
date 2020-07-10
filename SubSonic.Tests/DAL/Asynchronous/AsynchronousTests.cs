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
                people_all = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]",
                people_all_count = @"SELECT COUNT([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                people_equal = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] = @id_1)",
                people_greater_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] > @id_1)",
                people_less_than = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE ([T1].[ID] < @id_1)";

            Context.Database.Instance.AddCommandBehavior(people_all, cmd => People.ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_all_count, cmd => People.Count());
            Context.Database.Instance.AddCommandBehavior(people_greater_than, cmd => People.Where(x => x.ID > cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_equal, cmd => People.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(people_less_than, cmd => People.Where(x => x.ID < cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
        }

        [Test]
        public async Task ShouldBeAbleToGetSingleAsync()
        {
            Person person  = await Context.People.Where(x => x.ID == 1)
                .AsAsyncEnumerable()
                .SingleAsync();

            person.ID.Should().Be(1);
        }

        [Test]
        public async Task ShouldBeAbleToGetSingleAsyncWithPredicate()
        {
            Person person = await Context.People.SingleAsync(x => x.ID == 1);

            person.ID.Should().Be(1);
        }

        [Test]
        public void ShouldBeAbleToThrowWhenSingleAsyncHasMoreThanOne()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID > 0)
                  .AsAsyncEnumerable()
                  .SingleAsync();
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodFoundMoreThanOneResult.Format(nameof(SubSonicAsyncQueryable.SingleAsync))); ;
        }

        [Test]
        public void ShouldBeAbleToThrowWhenSingleOrDefaultAsyncHasMoreThanOne()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID > 0)
                  .AsAsyncEnumerable()
                  .SingleOrDefaultAsync();
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodFoundMoreThanOneResult.Format(nameof(SubSonicAsyncQueryable.SingleOrDefaultAsync))); ;
        }

        [Test]
        public void ShouldBeAbleToThrowWhenSingleAsyncHasMoreThanOneWithPredicate()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.SingleAsync(x => x.ID > 0);
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodFoundMoreThanOneResult.Format(nameof(SubSonicAsyncQueryable.SingleAsync))); ;
        }

        [Test]
        public void ShouldBeAbleToThrowWhenSingleOrDefaultAsyncHasMoreThanOneWithPredicate()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.SingleOrDefaultAsync(x => x.ID > 0);
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodFoundMoreThanOneResult.Format(nameof(SubSonicAsyncQueryable.SingleOrDefaultAsync))); ;
        }

        [Test]
        public void ShouldBeAbleToThrowSingleAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID == -1)
                .AsAsyncEnumerable()
                .SingleAsync();
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodDoesNotAllowNullValue.Format(nameof(SubSonicAsyncQueryable.SingleAsync)));
        }

        [Test]
        public void ShouldBeAbleToThrowSingleAsyncOnNullWithPredicate()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.SingleAsync(x => x.ID == -1);
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodDoesNotAllowNullValue.Format(nameof(SubSonicAsyncQueryable.SingleAsync)));
        }

        [Test]
        public async Task ShouldBeAbleToGetSingleOrDefaultAsync()
        {
            Person person = await Context.People.Where(x => x.ID == 1)
                .AsAsyncEnumerable()
                .SingleOrDefaultAsync();

            person.ID.Should().Be(1);
        }

        [Test]
        public async Task ShouldBeAbleToGetSingleOrDefaultAsyncWithPredicate()
        {
            Person person = await Context.People.SingleOrDefaultAsync(x => x.ID == 1);

            person.ID.Should().Be(1);
        }

        [Test]
        public void ShouldBeAbleToNotThrowSingleOrDefaultAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                Person person = await Context.People.Where(x => x.ID == -1)
                .AsAsyncEnumerable()
                .SingleOrDefaultAsync();

                person.Should().BeNull();

            }).Should().NotThrow();
        }

        [Test]
        public void ShouldBeAbleToNotThrowSingleOrDefaultAsyncOnNullWithPredicate()
        {
            FluentActions.Invoking(async () =>
            {
                Person person = await Context.People.SingleOrDefaultAsync(x => x.ID == -1);

                person.Should().BeNull();

            }).Should().NotThrow();
        }

        [Test]
        public async Task ShouldBeAbleToGetFirstAsync()
        {
            Person person = await Context.People.Where(x => x.ID > 0)
                .AsAsyncEnumerable()
                .FirstAsync();

            person.ID.Should().Be(1);
        }

        [Test]
        public async Task ShouldBeAbleToGetFirstAsyncWithPredicate()
        {
            Person person = await Context.People.FirstAsync(x => x.ID > 0);

            person.ID.Should().Be(1);
        }

        [Test]
        public void ShouldBeAbleToThrowFirstAsyncOnNull()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.Where(x => x.ID < 1)
                .AsAsyncEnumerable()
                .FirstAsync();
            })
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodDoesNotAllowNullValue.Format(nameof(SubSonicAsyncQueryable.FirstAsync)));
        }

        [Test]
        public void ShouldBeAbleToThrowFirstAsyncOnNullWithPredicate()
        {
            FluentActions.Invoking(async () =>
            {
                await Context.People.FirstAsync(x => x.ID < 1);
            }).Should()
                .Throw<InvalidOperationException>()
                .WithMessage(SubSonicErrorMessages.MethodDoesNotAllowNullValue.Format(nameof(SubSonicAsyncQueryable.FirstAsync))); ;
        }

        [Test]
        public async Task ShouldBeAbleToGetFirstOrDefaultAsync()
        {
            Person person = await Context.People.Where(x => x.ID > 2)
                .AsAsyncEnumerable()
                .FirstOrDefaultAsync();

            person.ID.Should().Be(3);
        }

        [Test]
        public async Task ShouldBeAbleToGetFirstOrDefaultAsyncWithPredicate()
        {
            Person person = await Context.People.FirstOrDefaultAsync(x => x.ID > 2);

            person.ID.Should().Be(3);
        }

        [Test]
        public void ShouldBeAbleToNotThrowFirstOrDefaultAsyncOnNullWithPredicate()
        {
            FluentActions.Invoking(async () =>
            {
                Person person = await Context.People.FirstOrDefaultAsync(x => x.ID < 1);

                person.Should().BeNull();

            }).Should().NotThrow();
        }

        [Test]
        public void ShouldBeAbleToLoadResultSet()
        {
            FluentActions.Invoking(async () =>
            {
                var cts = new CancellationTokenSource();

                int cnt = 0;

#if NETCOREAPP2_2 || NETCOREAPP2_1 || NETCOREAPP2_0
                var people = await Context.People.LoadAsync(cts.Token);

                await foreach(Person person in people
                    .WithCancellation(cts.Token)
                    .ConfigureAwait(true))
                {
                    person.FullName.Should().Be(String.Format("{0}, {1}{2}",
                        person.FamilyName, person.FirstName,
                        string.IsNullOrEmpty(person.MiddleInitial?.Trim()) ? "" : $" {person.MiddleInitial}."));

                    cnt++;
                }
#elif NETCOREAPP3_0 || NETCOREAPP3_1
                await foreach(var person in Context.People.LoadAsync(cts.Token))
                {
                    person.FullName.Should().Be(String.Format("{0}, {1}{2}",
                        person.FamilyName, person.FirstName,
                        string.IsNullOrEmpty(person.MiddleInitial?.Trim()) ? "" : $" {person.MiddleInitial}."));

                    cnt++;
                }
#endif

                cnt.Should().Be(Context.People.Count());

            }).Should().NotThrow();

        }
    }
}
