using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using Extensions.Test.Models;
    using Infrastructure;
    using Infrastructure.Logging;
    using Linq.Expressions;
    using Microsoft.Extensions.Logging;

    public partial class SqlQueryProviderTests
    {
        [Test]
        public void StringPropertyLengthCanBeUsed()
        {
            string expected = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE (LEN([T1].[FullName]) > @fullname_1)";

            Expression select = Context.People
                .Where(x =>
                    x.FullName.Length > 1)
                .Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Person>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void StringMethodTrimCanBeUsed()
        {
            string expected = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE (RTRIM(LTRIM([T1].[FullName])) = @fullname_1)";

            Expression select = Context.People
                .Where(x =>
                    x.FullName.Trim() == "Danvers, Kara Z.")
                .Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Person>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void StringMethodTrimEndCanBeUsed()
        {
            string expected = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE (RTRIM([T1].[FullName]) = @fullname_1)";

            Expression select = Context.People
                .Where(x =>
                    x.FullName.TrimEnd() == "Danvers, Kara Z.")
                .Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Person>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void StringMethodTrimStartCanBeUsed()
        {
            string expected = @"SELECT [T1].[ID], [T1].[FirstName], [T1].[MiddleInitial], [T1].[FamilyName], [T1].[FullName]
FROM [dbo].[Person] AS [T1]
WHERE (LTRIM([T1].[FullName]) = @fullname_1)";

            Expression select = Context.People
                .Where(x =>
                    x.FullName.TrimStart() == "Danvers, Kara Z.")
                .Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Person>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().Be(expected);
        }
    }
}
