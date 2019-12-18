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
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
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
WHERE ([{0}].[ID] = @ID) <> 0".Format("T1");

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
WHERE ([{0}].[IsAvailableStatus] = @IsAvailableStatus) <> 0".Format("T1");

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
        public void CanMergeMultipleWhereStatements()
        {
            string 
                units =
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = 1) <> 0".Format("T1"),
                status =
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
FROM [dbo].[Unit] AS [{0}]
WHERE (([{0}].[RealEstatePropertyID] = 1) AND ([{0}].[StatusID] = 1)) <> 0".Format("T1"),
                expected =
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
FROM [dbo].[Unit] AS [{0}]
WHERE (([{0}].[RealEstatePropertyID] = @RealEstatePropertyID) AND ([{0}].[StatusID] = @StatusID)) <> 0".Format("T1");

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
