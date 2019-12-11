using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using FluentAssertions;
    using Infrastructure;
    using System.Linq.Expressions;

    [TestFixture]
    public partial class SqlQueryProviderTests
        : BaseTestFixture
    {
        [Test]
        public async Task ShouldBeAbleToGenerateSelectSql()
        {
            Expression expression = DbContext.RealEstateProperties.Expression;

            object sql = await DbContext.Database.BuildSqlQuery<Models.RealEstateProperty>(SqlQueryType.Read, (sqlBuilder) =>
            {
                sqlBuilder.BuildSqlQuery(expression);

                return sqlBuilder.ToQueryObject();
            });

            sql.Should().NotBeNull();
        }
    }
}
