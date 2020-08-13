using FluentAssertions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NUnit.Framework;
using SubSonic.Core.Template.Testing.Objects;
using SubSonic.Extensions.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Template.Testing
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
    }
}
