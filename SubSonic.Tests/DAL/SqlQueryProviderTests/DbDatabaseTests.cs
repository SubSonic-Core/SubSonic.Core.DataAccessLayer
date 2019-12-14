using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    using FluentAssertions;
    using Infrastructure;
    
    [TestFixture]
    public partial class SqlQueryProviderTests
    {
        [Test]
        public void SqlBuilderCanGetDbQueryObjectWithNoParametersFromExpression()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]".Format("T1");

            ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

            IDbQueryObject dbQuery = builder.ToQueryObject(builder.BuildSelect());

            dbQuery.Should().NotBeNull();
            dbQuery.Sql.Should().Be(expected);
            dbQuery.Parameters.Should().BeEmpty();
        }

        [Test]
        public void SqlBuilderCanGetDbQueryObjectWithParametersFromExpression()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @ID) <> 0".Format("T1");

            ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

            IDbQueryObject dbQuery = builder.ToQueryObject(DbContext.Statuses.FindByID(1).Expression);

            dbQuery.Should().NotBeNull();
            dbQuery.Sql.Should().Be(expected);
            dbQuery.Parameters.Should().NotBeEmpty();
            dbQuery.Parameters.ElementAt(0).Value.Should().Be(1);
        }
    }
}
