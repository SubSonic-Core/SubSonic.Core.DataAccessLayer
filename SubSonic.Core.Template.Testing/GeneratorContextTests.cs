using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.SqlServer;
using SubSonic.Schema;
using System;
using System.Data.Common;

namespace SubSonic.CodeGenerator.Testing
{
    [TestFixture]
    public class GeneratorContextTests
    {
        private const string connection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DbSubSonic;Integrated Security=True";
        [Test]
        public void ShouldBeAbleToInstanciateTheGeneratorContext()
        {
            var context = new GeneratorContext(connection);

            Type providerFactoryType = typeof(SubSonicSqlClient);

            DbProviderFactories.TryGetFactory(providerFactoryType.Namespace, out DbProviderFactory factory);

            factory.Should().NotBeNull();
        }

        [Test]
        public void ShouldBeAbleToIdentifyEntityTypeAsView()
        {
            var context = new GeneratorContext(connection);

            IDbEntityModel model = context.Model.GetEntityModel<Models.Table>();

            model.DbObjectType.Should().Be(DbObjectTypeEnum.View);
        }

        [Test]
        public void DbSetCollectionGetSqlForViewFromView()
        {
            var context = new GeneratorContext(connection);

            context.Tables.ToString().Should().Be(Models.Table.SQL);
            
        }
    }
}
