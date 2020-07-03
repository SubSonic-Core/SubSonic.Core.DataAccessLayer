using FluentAssertions;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.OData
{
    using Infrastructure;
    using Infrastructure.Logging;
    using Linq.Expressions;
    using System;
    using System.Linq.Expressions;

    [TestFixture]
    public class ODataCompatibilityTests
        : SUT.BaseTestFixture
    {
        IODataQueryOptions<Models.Person> personQueryOptions = null;

        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            personQueryOptions = Substitute.For<IODataQueryOptions<Models.Person>>();
        }

        /// <summary>
        /// simulate an $filter
        /// </summary>
        /// <param name="direction"></param>
        [Test]
        [TestCase(nameof(string.StartsWith))]
        [TestCase(nameof(string.EndsWith))]
        [TestCase(nameof(string.Contains))]
        [TestCase(nameof(string.Equals))]
        public void ShouldBeAbleToApplyWhereUsingOData(string @operator)
        {
            personQueryOptions.ApplyTo(Arg.Any<IQueryable>())
                .Returns(call =>
                {
                    if (call.Arg<IQueryable>() is IQueryable<Models.Person> people)
                    {
                        switch (@operator)
                        {
                            case nameof(string.StartsWith):
                                return people
                                        .Where(x =>
                                            x.FamilyName.StartsWith("Car"));
                            case nameof(string.EndsWith):
                                return people
                                        .Where(x =>
                                            x.FamilyName.EndsWith("Car"));
                            case nameof(string.Contains):
                                return people
                                        .Where(x =>
                                            x.FamilyName.Contains("Car"));
                            case nameof(string.Equals):
                                return people
                                        .Where(x =>
                                            x.FamilyName == "Carter");
                        }
                    }

                    throw new NotImplementedException();
                });

            IQueryable select = personQueryOptions.ApplyTo(Context.People);

            select.Expression.Should().BeOfType<DbSelectExpression>();

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Models.Person>>();

                    query = builder.ToQuery(select.Expression);
                }).Should().NotThrow();
            }

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("WHERE");
            switch (@operator)
            {
                case nameof(string.StartsWith):
                case nameof(string.EndsWith):
                case nameof(string.Contains):
                    query.Sql.Should().Contain("LIKE");
                    break;
                case nameof(string.Equals):
                    query.Sql.Should().Contain(" = @");
                    break;
            }
        }

        /// <summary>
        /// simulate an $orderby descending and ascending
        /// </summary>
        /// <param name="direction"></param>
        [Test]
        [TestCase("ascending")]
        [TestCase("descending")]
        public void ShouldBeAbleToApplyOrderByUsingOData(string direction)
        {
            personQueryOptions.ApplyTo(Arg.Any<IQueryable>())
                .Returns(call =>
                {
                    if (call.Arg<IQueryable>() is IQueryable<Models.Person> people)
                    {
                        if (direction.Equals("ascending"))
                        {
                            return people.OrderBy(x => x.FamilyName);
                        }
                        else
                        {
                            return people.OrderByDescending(x => x.FamilyName);
                        }
                    }

                    throw new NotImplementedException();
                });

            IQueryable select = personQueryOptions.ApplyTo(Context.People);

            select.Expression.Should().BeOfType<DbSelectExpression>();

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Models.Person>>();

                    query = builder.ToQuery(select.Expression);
                }).Should().NotThrow();
            }

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("ORDER BY");

            if (direction.Equals("descending"))
            {
                query.Sql.Should().Contain("DESC");
            }
        }

        /// <summary>
        /// simulate an $orderby on more than two fields
        /// </summary>
        /// <param name="direction"></param>
        [Test]
        [TestCase("ascending")]
        [TestCase("descending")]
        public void ShouldBeAbleToApplyOrderByThenByUsingOData(string direction)
        {
            personQueryOptions.ApplyTo(Arg.Any<IQueryable>())
                .Returns(call =>
                {
                    if (call.Arg<IQueryable>() is IQueryable<Models.Person> people)
                    {
                        if (direction.Equals("ascending"))
                        {
                            return people
                                .OrderBy(x => x.FamilyName)
                                .ThenByDescending(x => x.FirstName);
                        }
                        else
                        {
                            return people
                                .OrderByDescending(x => x.FamilyName)
                                .ThenBy(x => x.FirstName);
                        }
                    }

                    throw new NotImplementedException();
                });

            IQueryable select = personQueryOptions.ApplyTo(Context.People);

            select.Expression.Should().BeOfType<DbSelectExpression>();

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Models.Person>>();

                    query = builder.ToQuery(select.Expression);
                }).Should().NotThrow();
            }

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("ORDER BY");

            if (direction.Equals("descending"))
            {
                query.Sql.Should().Contain("DESC");
            }
        }

        /// <summary>
        /// simulate a $top 
        /// </summary>
        /// <param name="top"></param>
        [Test]
        [TestCase(1)]
        [TestCase(10)]
        public void ShouldBeAbleToApplyTakeUsingOData(int top)
        {
            personQueryOptions.ApplyTo(Arg.Any<IQueryable>())
                .Returns(call =>
                {
                    if (call.Arg<IQueryable>() is IQueryable<Models.Person> people)
                    {
                        return people.Take(top);
                    }

                    throw new NotImplementedException();
                });

            IQueryable select = personQueryOptions.ApplyTo(Context.People);

            IDbQuery query = null;

            select.Expression.Should().BeOfType<DbSelectExpression>();

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Models.Person>>();

                    query = builder.ToQuery(select.Expression);
                }).Should().NotThrow();
            }

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain($"SELECT TOP ({top})");
        }

        /// <summary>
        /// simulate a $skip 
        /// </summary>
        /// <param name="top"></param>
        /// <remarks>
        /// skips by themselves do not make a lick of sense, but we should recognize that a skip was applied
        /// </remarks>
        [Test]
        [TestCase(1)]
        [TestCase(10)]
        public void ShouldBeAbleToApplySkipUsingOData(int skip)
        {
            personQueryOptions.ApplyTo(Arg.Any<IQueryable>())
                .Returns(call =>
                {
                    if (call.Arg<IQueryable>() is IQueryable<Models.Person> people)
                    {
                        return people.Skip(skip);
                    }

                    throw new NotImplementedException();
                });

            IQueryable select = personQueryOptions.ApplyTo(Context.People);

            select.Expression.Should().BeOfType<DbSelectExpression>();

            if (select.Expression is DbSelectExpression expression)
            {
                if (expression.Skip is ConstantExpression constant)
                {
                    constant.Value.Should().Be(skip);
                }
                else
                {
                    throw Error.InvalidOperation();
                }
            }
        }

        /// <summary>
        /// simulate a $skip and $top
        /// </summary>
        /// <param name="top"></param>
        /// <remarks>
        /// skips by themselves do not make a lick of sense, but we should recognize that a skip was applied
        /// </remarks>
        [Test]
        [TestCase(5, 10, true)]
        [TestCase(10, 0, false)]
        public void ShouldBeAbleToApplyTakeSkipUsingOData(int take, int skip, bool reverse)
        {
            personQueryOptions.ApplyTo(Arg.Any<IQueryable>())
                .Returns(call =>
                {
                    if (call.Arg<IQueryable>() is IQueryable<Models.Person> people)
                    {
                        if (!reverse)
                        {
                            return people.Take(take).Skip(skip);
                        }
                        else
                        {
                            return people.Skip(skip).Take(take);
                        }
                    }

                    throw new NotImplementedException();
                });

            IQueryable select = personQueryOptions.ApplyTo(Context.People);

            int
                pageNumber = skip / take;

            select.Expression.Should().BeOfType<DbSelectPageExpression>();

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Person> builder = Context.Instance.GetService<ISubSonicQueryProvider<Models.Person>>();

                    query = builder.ToQuery(select.Expression);
                }).Should().NotThrow();
            }

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().NotContain($"SELECT TOP ({take})");
            query.Parameters.Get("@PageSize").Value.Should().Be(take);
            query.Parameters.Get("@PageNumber").Value.Should().Be(pageNumber);
        }
    }
}
