using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using FluentAssertions;
    using Infrastructure.Logging;
    using Linq;
    using Linq.Expressions;
    using Microsoft.Extensions.Logging;

    [TestFixture]
    public partial class SqlQueryProviderTests
        : BaseTestFixture
    {
        [Test]
        public void ShouldBeAbleToGenerateSelectSql()
        {
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

            logging.LogInformation("\n\r" + sql);
        }
    }
}
