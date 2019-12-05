using FluentAssertions;
using NUnit.Framework;
using SubSonic.Data.DynamicProxies;
using SubSonic.Test.Rigging.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.DynamicProxyTests
{
    [TestFixture]
    public class DynamicProxyCreation
    {
        DbContext dbContext;

        [SetUp]
        public void SetupTestFixture()
        {
            dbContext = new TestDbContext();
        }

        [Test]
        public void CanBuildDynamicType()
        {
            var proxy = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            proxy.GetType().Should().BeDerivedFrom<RealEstateProperty>();
            proxy.GetType().GetField("_dbContextAccessor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(proxy).Should().BeOfType(typeof(DbContextAccessor));

            proxy.Units.Should().NotBeNull();
            
        }
    }
}
