using FluentAssertions;
using NUnit.Framework;
using SubSonic.Data.DynamicProxies;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using System.Collections.Generic;

namespace SubSonic.Tests.DAL.DynamicProxyTests
{
    using SUT;

    [TestFixture]
    public partial class DynamicProxyTests
        : BaseTestFixture
    { 
        [Test]
        public void CanBuildProxyForElegibleType()
        {
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<RealEstateProperty>(DbContext);

            proxyWrapper.IsElegibleForProxy.Should().BeTrue();
            proxyWrapper.Type.Should().BeDerivedFrom<RealEstateProperty>();

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.Should().BeAssignableTo<RealEstateProperty>();
        }

        [Test]
        public void DynamicProxyImplementsIEntityProxy()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            ((IEntityProxy)instance).Should().NotBeNull();
            ((IEntityProxy)instance).IsDirty.Should().BeFalse();
        }

        [Test]
        public void WillNotBuildProxyForInElegibleType()
        {
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<Status>(DbContext);

            proxyWrapper.IsElegibleForProxy.Should().BeFalse();
            proxyWrapper.Type.Should().BeNull();

            Status instance = DynamicProxy.CreateProxyInstanceOf<Status>(DbContext);

            instance.Should().BeAssignableTo<Status>();
        }

        [Test]
        public void ProxyNavigationPropertyWillSetForeignKeysOnSet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.StatusID.Should().Be(0);

            instance.Status = new Status() { ID = 1, Name = "Available" };

            instance.StatusID.Should().Be(1);
        }

        [Test]
        public void ProxyNavigationPropertyWillNotLoadWhenNullAndForiengKeyIsDefaultValueOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.Status.Should().BeNull();
        }

        [Test]
        public void ProxyNavigationPropertyWillLoadWhenNullAndForiengKeyIsNotDefaultValueOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.StatusID = 1;

            instance.Status.Should().NotBeNull();
        }

        [Test]
        public void ProxyCollectionPropertyWillLoadWhenNullOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.Units = null;

            instance.Units.Should().NotBeNull();
        }

        [Test]
        public void ProxyCollectionPropertyWillLoadWhenNotNullAndCountIsZeroOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.Units.Should().NotBeNull();
            instance.Units.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void ProxyCollectionPropertyWillNotLoadWhenNotNullAndCountGreaterThanZeroOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.Units = new HashSet<Unit>(new[] { DynamicProxy.CreateProxyInstanceOf<Unit>(DbContext) });

            instance.Units.Should().NotBeNull();
        }
    }
}
