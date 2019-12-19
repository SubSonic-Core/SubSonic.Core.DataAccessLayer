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
    using System.Data;

    [TestFixture]
    public partial class SqlQueryProviderTests
    {
        [Test]
        public void CanGetDbQueryObjectWithNoParametersFromExpression()
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
        public void CanGetDbQueryObjectWithParametersFromExpression()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @ID)".Format("T1");

            ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

            IDbQueryObject dbQuery = builder.ToQueryObject(DbContext.Statuses.FindByID(1).Expression);

            dbQuery.Should().NotBeNull();
            dbQuery.Sql.Should().Be(expected);
            dbQuery.Parameters.Should().NotBeEmpty();
            dbQuery.Parameters.ElementAt(0).Value.Should().Be(1);
            ((DbType)dbQuery.Parameters.ElementAt(0).DbType).Should().Be(DbType.Int32);
            dbQuery.Parameters.ElementAt(0).SourceColumn.Should().Be("ID");
        }
    }
}
