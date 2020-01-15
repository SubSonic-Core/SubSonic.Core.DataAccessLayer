using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubSonic.Extensions.Test.Models;
using System.Data;
using System.Linq.Expressions;
using FluentAssertions;

namespace SubSonic.Tests.DAL.SqlQueryProvider
{
    
    using Infrastructure;
    

    [TestFixture]
    public partial class SqlQueryProviderTests
    {
        [Test]
        public void CanCompileDbSetQuery()
        {
            object set = Expression.Lambda(DbContext.RealEstateProperties.Expression).Compile().DynamicInvoke();

            set.Should().BeSameAs(DbContext.RealEstateProperties);
        }

        [Test]
        public void CanGetDbQueryObjectWithNoParametersFromExpression()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]".Format("T1");

            ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

            IDbQuery dbQuery = builder.ToQuery(DbContext.Statuses.Expression);

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
WHERE ([{0}].[ID] = @id_1)".Format("T1");

            ISubSonicQueryProvider<Status> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<Status>>();

            IDbQuery dbQuery = builder.ToQuery(DbContext.Statuses.FindByID(1).Expression);

            dbQuery.Should().NotBeNull();
            dbQuery.Sql.Should().Be(expected);
            dbQuery.Parameters.Should().NotBeEmpty();
            dbQuery.Parameters.ElementAt(0).Value.Should().Be(1);
            TypeConvertor.ToSqlDbType(dbQuery.Parameters.ElementAt(0).DbType).Should().Be(SqlDbType.Int);
            dbQuery.Parameters.ElementAt(0).SourceColumn.Should().Be("ID");
        }
    }
}
