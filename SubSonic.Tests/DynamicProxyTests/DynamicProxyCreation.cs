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
        public void CanBuildProxyForElegibleType()
        {
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<RealEstateProperty>();

            proxyWrapper.IsElegibleForProxy.Should().BeTrue();
            proxyWrapper.Type.Should().BeDerivedFrom<RealEstateProperty>();
        }

        [Test]
        public void WillNotBuildProxyForInElegibleType()
        {
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<Status>();

            proxyWrapper.IsElegibleForProxy.Should().BeFalse();
            proxyWrapper.Type.Should().BeNull();
        }
    }
}
