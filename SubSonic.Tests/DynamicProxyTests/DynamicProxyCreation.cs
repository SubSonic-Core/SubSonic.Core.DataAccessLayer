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
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<RealEstateProperty>(dbContext);

            proxyWrapper.IsElegibleForProxy.Should().BeTrue();
            proxyWrapper.Type.Should().BeDerivedFrom<RealEstateProperty>();

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.Should().BeAssignableTo<RealEstateProperty>();
        }

        [Test]
        public void WillNotBuildProxyForInElegibleType()
        {
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<Status>(dbContext);

            proxyWrapper.IsElegibleForProxy.Should().BeFalse();
            proxyWrapper.Type.Should().BeNull();

            Status instance = DynamicProxy.CreateProxyInstanceOf<Status>(dbContext);

            instance.Should().BeAssignableTo<Status>();
        }

        [Test]
        public void ProxyNavigationPropertyWillSetForeignKeysOnSet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.StatusID.Should().Be(0);

            instance.Status = new Status() { ID = 1, Name = "Available" };

            instance.StatusID.Should().Be(1);
        }

        [Test]
        public void ProxyNavigationPropertyWillNotLoadWhenNullAndForiengKeyIsDefaultValueOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.Status.Should().BeNull();
        }

        [Test]
        public void ProxyNavigationPropertyWillLoadWhenNullAndForiengKeyIsNotDefaultValueOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.StatusID = 1;

            instance.Status.Should().NotBeNull();
        }

        [Test]
        public void ProxyCollectionPropertyWillLoadWhenNullOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.Units = null;

            instance.Units.Should().NotBeNull();
        }

        [Test]
        public void ProxyCollectionPropertyWillLoadWhenNotNullAndCountIsZeroOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.Units.Should().NotBeNull();
            instance.Units.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void ProxyCollectionPropertyWillNotLoadWhenNotNullAndCountGreaterThanZeroOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(dbContext);

            instance.Units = new HashSet<Unit>(new[] { DynamicProxy.CreateProxyInstanceOf<Unit>(dbContext) });

            instance.Units.Should().NotBeNull();
        }
    }
}
