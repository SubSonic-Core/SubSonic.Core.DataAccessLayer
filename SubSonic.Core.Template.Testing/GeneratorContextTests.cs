using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SubSonic.Extensions.SqlServer;
using SubSonic.Logging;
using SubSonic.Schema;
using System;
using System.Data.Common;
using System.Linq.Expressions;

namespace SubSonic.CodeGenerator.Testing
{
    [TestFixture]
    public class GeneratorContextTests
    {
        private const string connection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DbSubSonic;Integrated Security=True";
        [Test]
        public void ShouldBeAbleToInstanciateTheGeneratorContext()
        {
            using var context = new GeneratorContext(connection);

            Type providerFactoryType = typeof(SubSonicSqlClient);

            DbProviderFactories.TryGetFactory(providerFactoryType.Namespace, out DbProviderFactory factory);

            factory.Should().NotBeNull();
        }

        [Test]
        public void ShouldBeAbleToIdentifyEntityTypeAsView()
        {
            using var context = new GeneratorContext(connection);

            IDbEntityModel model = context.Model.GetEntityModel<Models.Table>();

            model.DbObjectType.Should().Be(DbObjectTypeEnum.View);
        }

        [Test]
        public void DbSetCollectionForTablesGetSqlFromView()
        {
            using var context = new GeneratorContext(connection);

            string expected = $@"SELECT [T1].[Schema], [T1].[Name]
FROM ({Models.Table.SQL}) AS [T1]";
            
            Expression select = context.Tables.Expression;

            IDbQuery query = null;

            var logging = context.Instance.GetService<ISubSonicLogger<ISubSonicCollection<Models.Table>>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Table> builder = context.Instance.GetService<ISubSonicQueryProvider<Models.Table>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Be(expected);
        }

        [Test]
        public void DbSetCollectionForRelationshipsGetSqlFromView()
        {
            using var context = new GeneratorContext(connection);

            string expected = $@"SELECT [T1].[TableName], [T1].[ColumnName], [T1].[ForiegnTableName], [T1].[ForiegnColumnName], [T1].[ConstraintName], [T1].[SchemaOwner]
FROM ({Models.Relationship.SQL}) AS [T1]";

            Expression select = context.Relationships.Expression;

            IDbQuery query = null;

            var logging = context.Instance.GetService<ISubSonicLogger<ISubSonicCollection<Models.Table>>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Relationship> builder = context.Instance.GetService<ISubSonicQueryProvider<Models.Relationship>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Be(expected);
        }
    }
}
