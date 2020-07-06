using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.ScalarFunctions
{
    using Extensions.Test.Data.Functions;
    using Infrastructure;
    using Infrastructure.Logging;
    using Linq.Expressions;
    using SubSonic.Extensions.Test.Models;
    using SUT;

    [TestFixture]
    public class ScalarFunctionTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            Logger = Context.Instance.GetService<ISubSonicLogger<ScalarFunctionTests>>();
        }

        [Test]
        public void ShouldBeAbleToMapStaticScalarFunctionToDbScalarFunction()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([dbo].[IsPropertyAvailable]([{0}].[StatusID]) = @b_value_1)".Format("T1");

            Expression select = Context
                .RealEstateProperties
                .Where(rep =>
                    Scalar.IsPropertyAvailable(rep.StatusID) == true)
                .Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<RealEstateProperty> builder = Context.Instance.GetService<ISubSonicQueryProvider<RealEstateProperty>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Should().NotBeNull();
            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Contain("[dbo].[IsPropertyAvailable]([T1].[StatusID])");

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@b_value_1").Value.Should().Be(true);
        }

        [Test]
        public void ShouldBeAbleToRenderMultipleArguments()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([dbo].[SupportsMultipleArguments]([{0}].[StatusID], @b_value_1) = @b_value_2)".Format("T1");

            Expression select = Context
                .RealEstateProperties
                .Where(rep =>
                    Scalar.SupportsMultipleArguments(rep.StatusID, false) == true)
                .Expression;

            IDbQuery query = null;

            var logging = Context.Instance.GetService<ISubSonicLogger<DbSelectExpression>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<RealEstateProperty> builder = Context.Instance.GetService<ISubSonicQueryProvider<RealEstateProperty>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Should().NotBeNull();
            query.Sql.Should().NotBeNullOrEmpty();

            logging.LogInformation("\n" + query.Sql + "\n");

            query.Sql.Should().Be(expected);

            query.Parameters.Should().NotBeEmpty();
            query.Parameters.Get("@b_value_1").Value.Should().Be(false);
            query.Parameters.Get("@b_value_2").Value.Should().Be(true);
        }
    }
}
