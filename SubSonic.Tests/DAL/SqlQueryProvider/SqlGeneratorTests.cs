using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Data;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using Data.DynamicProxies;
    using Extensions.Test;
    using Extensions.Test.Models;
    using Infrastructure;
    using Infrastructure.Logging;
    using Infrastructure.Schema;
    using Linq;
    using Linq.Expressions;

    [TestFixture]
    public partial class SqlQueryProviderTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();
        }

        [Test]
        public void CanGenerateSelectSqlWithInConstraint()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE [{0}].[StatusID] IN (@el_1, @el_2, @el_3)".Format("T1");

            Expression select = DbContext
                .RealEstateProperties
                .Where(rep =>
                    rep.StatusID.In(new[] { 1, 2, 3 }))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] IN (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@el_1").Value.Should().Be(1);
            query.Parameters.Get("@el_2").Value.Should().Be(2);
            query.Parameters.Get("@el_3").Value.Should().Be(3);
        }

        [Test]
        public void CanGenerateSelectSqlWithInSelectConstraint()
        {
            string expected =
@"SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]
WHERE [T1].[StatusID] IN (
	SELECT [T2].[ID]
	FROM [dbo].[Status] AS [T2]
	WHERE ([T2].[IsAvailableStatus] = @b_isavailablestatus_1))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .Where(rep =>
                    rep.StatusID.In(
                        DbContext
                            .Statuses
                            .Where(stat => stat.IsAvailableStatus == true)
                            .Select(x => x.ID)))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] IN (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            ((bool)query.Parameters.Get("@b_isavailablestatus_1").Value).Should().BeTrue();
        }

        [Test]
        public void CanGenerateSelectSqlWithNotInSelectConstraint()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE [{0}].[StatusID] NOT IN (
	SELECT [{1}].[ID]
	FROM [dbo].[Status] AS [{1}]
	WHERE ([{1}].[IsAvailableStatus] = @b_isavailablestatus_1))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .Where(rep =>
                    rep.StatusID.NotIn(
                        DbContext
                            .Statuses
                            .Where(stat => stat.IsAvailableStatus == true)
                            .Select(x => x.ID)))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] NOT IN (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            ((bool)query.Parameters.Get("@b_isavailablestatus_1").Value).Should().BeTrue();
        }

        [Test]
        public void CanGenerateSelectSqlWithNotInConstraint()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE [{0}].[StatusID] NOT IN (@el_1, @el_2, @el_3)".Format("T1");

            Expression select = DbContext
                .RealEstateProperties
                .Where(rep =>
                    rep.StatusID.NotIn(new[] { 1, 2, 3 }))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] NOT IN (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@el_1").Value.Should().Be(1);
            query.Parameters.Get("@el_2").Value.Should().Be(2);
            query.Parameters.Get("@el_3").Value.Should().Be(3);
        }

        [Test]
        public void CanGenerateSelectSqlForRealEstateProperty()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]".Format("T1");

            Expression expression = DbContext.RealEstateProperties.Select().Expression;

            expression.Should().BeOfType<DbSelectExpression>();

            DbSelectExpression dbSelect = (DbSelectExpression)expression;

            string sql = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    sql = dbSelect.QueryText;
                }).Should().NotThrow();
            }

            sql.Should().NotBeNullOrEmpty();
            sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + sql + "\n");

            sql.Should().Be(expected);

            sql.Should().Be(DbContext.RealEstateProperties.Select().Expression.ToString());
        }

        [Test]
        public void CanGenerateSelectSqlForStatus()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]".Format("T1");

            Expression expression = DbContext.Statuses.Select().Expression;

            expression.Should().BeOfType<DbSelectExpression>();

            DbSelectExpression dbSelect = (DbSelectExpression)expression;

            string sql = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    sql = dbSelect.QueryText;
                }).Should().NotThrow();
            }

            sql.Should().NotBeNullOrEmpty();
            sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + sql + "\n");

            sql.Should().Be(expected);

            sql.Should().Be(DbContext.Statuses.Select().Expression.ToString());
        }

        [Test]
        public void CanGenerateSelectSqlForUnit()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]".Format("T1");

            Expression expression = DbContext.Units.Select().Expression;

            expression.Should().BeOfType<DbSelectExpression>();

            DbSelectExpression dbSelect = (DbSelectExpression)expression;

            string sql = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    sql = dbSelect.QueryText;
                }).Should().NotThrow();
            }

            sql.Should().NotBeNullOrEmpty();
            sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + sql + "\n");

            sql.Should().Be(expected);

            sql.Should().Be(DbContext.Units.Select().Expression.ToString());
        }

        [Test]
        public void CanGenerateSelectWithConstraintsUsingFindByID()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @id_1)".Format("T1");

            Expression expression = DbContext.Statuses.FindByID(1).Expression;

            expression.Should().BeOfType<DbSelectExpression>();

            DbSelectExpression dbSelect = (DbSelectExpression)expression;

            string sql = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    sql = dbSelect.QueryText;
                }).Should().NotThrow();
            }

            sql.Should().NotBeNullOrEmpty();
            sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + sql + "\n");

            sql.Should().Be(expected);
        }

        [Test]
        public void CanGenerateSelectWithKeyColumnWithContraints()
        {
            string expected =
@"SELECT [{0}].[ID]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[IsAvailableStatus] = @b_isavailablestatus_1)".Format("T1");

            Expression select = DbContext
                .Statuses
                .Where(Status => Status.IsAvailableStatus == true)
                .Select(Status => Status.ID)
                .Expression;

            select.Should().BeOfType<DbSelectExpression>();

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Should().NotBeNull();

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.ElementAt(0).ParameterName.Should().Be("@b_isavailablestatus_1");
            TypeConvertor.ToSqlDbType(query.Parameters.ElementAt(0).DbType).Should().Be(SqlDbType.Bit);
        }

        [Test]
        public void CanGenerateSqlForWhereExists()
        {
            string
                expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE EXISTS (
	SELECT [{1}].[ID]
	FROM [dbo].[Unit] AS [{1}]
	WHERE (([{1}].[RealEstatePropertyID] = [{0}].[ID]) AND ([{1}].[Bedrooms] = @bedrooms_1)))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .WhereExists((property) => 
                    DbContext.Units
                        .Where(unit => unit.RealEstatePropertyID == property.ID && unit.NumberOfBedrooms == 2)
                        .Select(unit => unit.ID))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("WHERE EXISTS (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@bedrooms_1").Value.Should().Be(2);
        }

        [Test]
        public void CanGenerateSqlForWhereNotExists()
        {
            string
                expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE NOT EXISTS (
	SELECT [{1}].[ID]
	FROM [dbo].[Unit] AS [{1}]
	WHERE (([{1}].[RealEstatePropertyID] = [{0}].[ID]) AND ([{1}].[Bedrooms] = @bedrooms_1)))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .WhereNotExists((Property) =>
                    DbContext.Units
                        .Where(x => x.RealEstatePropertyID == Property.ID && x.NumberOfBedrooms == 1)
                        .Select(x => x.ID))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("WHERE NOT EXISTS (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@bedrooms_1").Value.Should().Be(1);
        }

        [Test]
        public void CanMergeMultipleWhereStatements()
        {
            string 
                units =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = 1)".Format("T1"),
                status =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE (([{0}].[RealEstatePropertyID] = 1) AND ([{0}].[StatusID] = 1))".Format("T1"),
                expected =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE (([{0}].[RealEstatePropertyID] = @realestatepropertyid_1) AND ([{0}].[StatusID] = @statusid_2))".Format("T1");

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.ID = 1;
            instance.StatusID = 1;

            DbContext.Database.Instance.AddCommandBehavior(
                units,
                Units.Where(x => x.RealEstatePropertyID == 1));

            DbContext.Database.Instance.AddCommandBehavior(
                status,
                Units.Where(x => x.RealEstatePropertyID == 1 && x.StatusID == 1));

            Expression select = ((ISubSonicCollection<Unit>)instance.Units.Where(Unit => Unit.StatusID == 1)).Expression;

            select.Should().NotBeNull();

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Count.Should().Be(2);
        }

        [Test]
        public void CanGenerateBetweenDateComparison()
        {
            string expected =
@"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
WHERE [{0}].[StartDate] BETWEEN @dt_start_1 AND @dt_end_2".Format("T1");

            DateTime
                Start = new DateTime(1985, 01, 01),
                End = DateTime.Now;

            Expression select = DbContext
                .Renters
                .Where(Renter =>
                    Renter.StartDate.Between(Start, End))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] BETWEEN ");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@dt_start_1").Value.Should().Be(Start);
            query.Parameters.Get("@dt_end_2").Value.Should().Be(End);
        }

        [Test]
        public void CanGenerateNotBetweenDateComparison()
        {
            string expected =
@"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
WHERE [{0}].[StartDate] NOT BETWEEN @dt_start_1 AND @dt_end_2".Format("T1");

            DateTime
                Start = new DateTime(1985, 01, 01),
                End = DateTime.Now;

            Expression select = DbContext
                .Renters
                .Where(Renter =>
                    Renter.StartDate.NotBetween(Start, End))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] NOT BETWEEN ");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@dt_start_1").Value.Should().Be(Start);
            query.Parameters.Get("@dt_end_2").Value.Should().Be(End);
        }

        [Test]
        public void CanGenerateForDateBetweenStartAndEndComparison()
        {
            string expected =
@"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
WHERE @dt_now_1 BETWEEN [{0}].[StartDate] AND ISNULL([{0}].[EndDate], @dt_default_2)".Format("T1");

            DateTime
                Now = DateTime.Now,
                Default = Now.AddDays(1).Date;

            Expression select = DbContext
                .Renters
                .Where(Renter =>
                    Now.Between(Renter.StartDate, Renter.EndDate.IsNull(Default)))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("@dt_now_1 BETWEEN [");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@dt_now_1").Value.Should().Be(Now);
            query.Parameters.Get("@dt_default_2").Value.Should().Be(Default);
        }

        [Test]
        public void CanGenerateForDateNotBetweenStartAndEndComparison()
        {
            string expected =
@"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
WHERE @dt_now_1 NOT BETWEEN [{0}].[StartDate] AND ISNULL([{0}].[EndDate], @dt_default_2)".Format("T1");

            DateTime
                Now = DateTime.Now,
                Default = Now.AddDays(1).Date;

            Expression select = DbContext
                .Renters
                .Where(Renter =>
                    Now.NotBetween(Renter.StartDate, Renter.EndDate.IsNull(Default)))
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("@dt_now_1 NOT BETWEEN [");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@dt_now_1").Value.Should().Be(Now);
            query.Parameters.Get("@dt_default_2").Value.Should().Be(Default);
        }

        [Test]
        public void CanGenerateSelectDistinctSql()
        {
            string expected =
@"SELECT DISTINCT [{0}].[PersonID]
FROM [dbo].[Renter] AS [{0}]".Format("T1");

            Expression select = DbContext
                .Renters
                .Distinct()
                .Select(x => x.PersonID)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("SELECT DISTINCT");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void CanGenerateSelectTopSql()
        {
            string expected =
@"SELECT TOP (1) [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]".Format("T1");

            Expression select = DbContext
                .Renters
                .Take(1)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("SELECT TOP (1)");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void CanGenerateSelectQueryWithPagination()
        {
            string expected =
@"WITH page AS
(
	SELECT [{0}].[PersonID], [{0}].[UnitID]
	FROM [dbo].[Renter] AS [{0}]
	ORDER BY [{0}].[PersonID], [{0}].[UnitID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
	INNER JOIN page
		ON (([page].[PersonID] = [{0}].[PersonID]) AND ([page].[UnitID] = [{0}].[UnitID]))
OPTION (RECOMPILE)".Format("T1");

            Expression select = DbContext
                .Renters
                .Page(default(int), 20)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectPageExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }



        [Test]
        public void CanGenerateSqlForRealEstatePropertyPageQuery()
        {
            string expected =
@"SELECT COUNT([{0}].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [{0}];
WITH page AS
(
	SELECT [{0}].[ID]
	FROM [dbo].[RealEstateProperty] AS [{0}]
	ORDER BY [{0}].[ID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
	INNER JOIN page
		ON ([page].[ID] = [{0}].[ID])
OPTION (RECOMPILE)".Format("T1");

            Expression select = DbContext
                .RealEstateProperties
                .Page(default(int), 5)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectPageExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToPagedQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void CanGenerateSqlForRealEstatePropertyPageQueryWithWhereClause()
        {
            string expected =
@"SELECT COUNT([{0}].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[HasParallelPowerGeneration] = @hasparallelpowergeneration_1);
WITH page AS
(
	SELECT [{0}].[ID]
	FROM [dbo].[RealEstateProperty] AS [{0}]
	WHERE ([{0}].[HasParallelPowerGeneration] = @hasparallelpowergeneration_1)
	ORDER BY [{0}].[ID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
	INNER JOIN page
		ON ([page].[ID] = [{0}].[ID])
OPTION (RECOMPILE)".Format("T1");

            Expression select = DbContext
                .RealEstateProperties
                .Where((property) =>
                    property.HasParallelPowerGeneration == true)
                .Page(default(int), 5)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectPageExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToPagedQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void CanGenerateSqlForRealEstatePropertyPageQueryWithWhereAndOrderByClause()
        {
            string expected =
@"SELECT COUNT([{0}].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[HasParallelPowerGeneration] = @hasparallelpowergeneration_1);
WITH page AS
(
	SELECT [{0}].[ID]
	FROM [dbo].[RealEstateProperty] AS [{0}]
	WHERE ([{0}].[HasParallelPowerGeneration] = @hasparallelpowergeneration_1)
	ORDER BY [{0}].[ID], [{0}].[StatusID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
	INNER JOIN page
		ON ([page].[ID] = [{0}].[ID])
OPTION (RECOMPILE)".Format("T1");

            Expression select = DbContext
                .RealEstateProperties
                .Where((property) =>
                    property.HasParallelPowerGeneration == true)
                .OrderBy((property) => property.ID)
                .ThenOrderBy((property) => property.StatusID)
                .Page(default(int), 5)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectPageExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToPagedQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }

        [Test]
        public void CanGenerateSqlForRealEstatePropertyPageQueryWithWhereAndOrderByDescendingClause()
        {
            string expected =
@"SELECT COUNT([{0}].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[HasParallelPowerGeneration] = @hasparallelpowergeneration_1);
WITH page AS
(
	SELECT [{0}].[ID]
	FROM [dbo].[RealEstateProperty] AS [{0}]
	WHERE ([{0}].[HasParallelPowerGeneration] = @hasparallelpowergeneration_1)
	ORDER BY [{0}].[ID] DESC, [{0}].[StatusID] DESC
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
	INNER JOIN page
		ON ([page].[ID] = [{0}].[ID])
OPTION (RECOMPILE)".Format("T1");

            Expression select = DbContext
                .RealEstateProperties
                .Where((property) =>
                    property.HasParallelPowerGeneration == true)
                .OrderByDescending((property) => property.ID)
                .ThenOrderByDescending((property) => property.StatusID)
                .Page(default(int), 5)
                .Expression;

            IDbQuery query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectPageExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToPagedQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);
        }
    }
}
