using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using Extensions.Test.Models;
    using Linq;
    using Linq.Expressions;
    using Logging;
    using Microsoft.Extensions.Logging;
    using SubSonic.src;
    using System;

    [TestFixture]
    public partial class SqlQueryProviderTests
    {
        class PersonRent
        {
            public string FullName { get; set; }

            public decimal Rent { get; set; }
        }

        class UnitOfProperty
        {
            public bool? HasParallelPowerGeneration { get; internal set; }
            public string Status { get; internal set; }
        }

        [Test]
        public void MemberInitializationThrowWhenMissingTableReference()
        {
            FluentActions.Invoking(() =>
            {
                Expression select = Context
                .Renters
                .Where(x =>
                   DateTime.Today.Between(x.StartDate, x.EndDate.GetValueOrDefault(DateTime.Today)))
                .Select(x => new PersonRent()
                {
                    FullName = x.Person.FullName,
                    Rent = x.Rent
                }).Expression;
            }).Should().Throw<InvalidOperationException>().WithMessage(SubSonicErrorMessages.MissingTableReferenceFor.Format(nameof(Person)));
        }

        [Test]
        public void MemberInitializationWithThenIncludeTests()
        {
            string expected = @"SELECT [T1].[HasParallelPowerGeneration], [T2].[Name] AS [Status]
FROM [dbo].[Unit] AS [T3]
	INNER JOIN [dbo].[RealEstateProperty] AS [T1]
		ON ([T1].[ID] = [T3].[RealEstatePropertyID])
	INNER JOIN [dbo].[Status] AS [T2]
		ON ([T2].[ID] = [T1].[StatusID])";

            Expression select = Context
                .Units
                .Include(x => x.RealEstateProperty)
                .ThenInclude(x => x.Status)
                .Select(x => new UnitOfProperty()
                {
                    HasParallelPowerGeneration = x.RealEstateProperty.HasParallelPowerGeneration,
                    Status = x.RealEstateProperty.Status.Name
                }).Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Person>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();

                logging.LogInformation(query.Sql);
            }

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void MemberInitializationWithIncludeTests()
        {
            string expected = @"SELECT [T1].[FullName], [T2].[Rent]
FROM [dbo].[Renter] AS [T2]
	INNER JOIN [dbo].[Person] AS [T1]
		ON ([T1].[ID] = [T2].[PersonID])
WHERE @dt_value_1 BETWEEN [T2].[StartDate] AND COALESCE([T2].[EndDate], @dt_value_2)";

            Expression select = Context
                .Renters
                .Where(x =>
                   DateTime.Today.Between(x.StartDate, x.EndDate.GetValueOrDefault(DateTime.Today)))
                .Include(x => x.Person)
                .Select(x => new PersonRent()
                {
                    FullName = x.Person.FullName,
                    Rent = x.Rent
                }).Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Person>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();

                logging.LogInformation(query.Sql);
            }

            query.Sql.Should().Be(expected);
        }
    }
}
