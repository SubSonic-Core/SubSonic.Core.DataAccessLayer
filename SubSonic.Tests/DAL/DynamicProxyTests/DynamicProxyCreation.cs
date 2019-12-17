using FluentAssertions;
using NUnit.Framework;
using SubSonic.Data.DynamicProxies;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using System.Collections.Generic;

namespace SubSonic.Tests.DAL.DynamicProxyTests
{
    using SubSonic.Linq;
    using SUT;
    using System.Linq;

    [TestFixture]
    public partial class DynamicProxyTests
        : BaseTestFixture
    { 
        [Test]
        public void BuildProxyForElegibleType()
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
            
            ((IEntityProxy)instance).IsNew.Should().BeTrue();
            ((IEntityProxy)instance).IsDirty.Should().BeFalse();

            ((IEntityProxy)instance).IsNew = false;

            ((IEntityProxy)instance).IsNew.Should().BeFalse();

            ((IEntityProxy)instance).OnPropertyChange((IEntityProxy)instance);

            ((IEntityProxy)instance).IsDirty.Should().BeTrue();

            ((IEntityProxy<RealEstateProperty>)instance).Data.Should().BeSameAs(instance);
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
        public void ProxyCollectionPropertyWillNotBeNullOnGet()
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
            // have yet to hit the db
            instance.Units.Count.Should().Be(0);
            instance.Units.AsQueryable().Load();
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
