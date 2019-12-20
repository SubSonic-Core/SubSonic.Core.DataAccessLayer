using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SubSonic.Extensions.Test.Models;
using SubSonic.Tests.DAL.SUT;
using System.Data;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using Data.DynamicProxies;
    using Infrastructure.Logging;
    using Linq;
    using Linq.Expressions;
    using SubSonic.Extensions.Test;
    using SubSonic.Infrastructure;

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

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
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
	WHERE ([T2].[IsAvailableStatus] = @IsAvailableStatus))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .Where(rep =>
                    rep.StatusID.In(
                        DbContext
                            .Statuses
                            .Where(stat => stat.IsAvailableStatus == true)
                            .Select(x => x.ID)))
                .Expression;

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] IN (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            ((bool)query.Parameters.Get("@IsAvailableStatus").Value).Should().BeTrue();
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
	WHERE ([{1}].[IsAvailableStatus] = @IsAvailableStatus))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .Where(rep =>
                    rep.StatusID.NotIn(
                        DbContext
                            .Statuses
                            .Where(stat => stat.IsAvailableStatus == true)
                            .Select(x => x.ID)))
                .Expression;

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("] NOT IN (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            ((bool)query.Parameters.Get("@IsAvailableStatus").Value).Should().BeTrue();
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

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
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
WHERE ([{0}].[ID] = @ID)".Format("T1");

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
WHERE ([{0}].[IsAvailableStatus] = @IsAvailableStatus)".Format("T1");

            Expression select = DbContext
                .Statuses
                .Where(Status => Status.IsAvailableStatus == true)
                .Select(Status => Status.ID)
                .Expression;

            select.Should().BeOfType<DbSelectExpression>();

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
                }).Should().NotThrow();
            }

            query.Should().NotBeNull();

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.ElementAt(0).ParameterName.Should().Be("@IsAvailableStatus");
            ((DbType)query.Parameters.ElementAt(0).DbType).Should().Be(DbType.Boolean);
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
	WHERE (([{1}].[RealEstatePropertyID] = [{0}].[ID]) AND ([{1}].[Bedrooms] = @Bedrooms)))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .WhereExists((property) => 
                    DbContext.Units
                        .Where(unit => unit.RealEstatePropertyID == property.ID && unit.NumberOfBedrooms == 2)
                        .Select(unit => unit.ID))
                .Expression;

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("WHERE EXISTS (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@Bedrooms").Value.Should().Be(2);
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
	WHERE (([{1}].[RealEstatePropertyID] = [{0}].[ID]) AND ([{1}].[Bedrooms] = @Bedrooms)))".Format("T1", "T2");

            Expression select = DbContext
                .RealEstateProperties
                .WhereNotExists((Property) =>
                    DbContext.Units
                        .Where(x => x.RealEstatePropertyID == Property.ID && x.NumberOfBedrooms == 1)
                        .Select(x => x.ID))
                .Expression;

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("WHERE NOT EXISTS (");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@Bedrooms").Value.Should().Be(1);
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
WHERE (([{0}].[RealEstatePropertyID] = @RealEstatePropertyID) AND ([{0}].[StatusID] = @StatusID))".Format("T1");

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

            IDbQueryObject query = null;

            var logging = DbContext.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

                    query = builder.ToQueryObject(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().StartWith("SELECT");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Count.Should().Be(2);
        }
    }
}
