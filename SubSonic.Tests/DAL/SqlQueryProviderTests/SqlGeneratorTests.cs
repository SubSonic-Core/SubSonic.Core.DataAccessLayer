using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using FluentAssertions;
    using Infrastructure;
    using Linq;
    using Linq.Expressions;

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

            FluentActions.Invoking(() =>
            {
                sql = dbSelect.QueryText;
            }).Should().NotThrow();

            sql.Should().NotBeNullOrEmpty();
            sql.Should().StartWith("SELECT");
        }
    }
}
