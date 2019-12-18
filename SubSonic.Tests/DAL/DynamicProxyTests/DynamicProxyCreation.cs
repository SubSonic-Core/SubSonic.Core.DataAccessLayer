using FluentAssertions;
using NUnit.Framework;
using SubSonic.Data.DynamicProxies;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Tests.DAL.DynamicProxyTests
{
    using Extensions.Test;
    using Linq;
    using SUT;

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

            ((IEntityProxy)instance).KeyData.Should().NotBeEmpty();
            ((IEntityProxy)instance).KeyData.Should().BeEquivalentTo(new object[] { 0 });

            ((IEntityProxy)instance).IsNew.Should().BeTrue();
            ((IEntityProxy)instance).IsNew = false;
            ((IEntityProxy)instance).IsNew.Should().BeFalse();

            ((IEntityProxy)instance).IsDirty.Should().BeFalse();
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
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @ID) <> 0".Format("T1");

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            DbContext.Database.Instance.AddCommandBehavior(expected, Statuses.Where(x => x.ID == 1));

            instance.StatusID = 1;

            instance.Status.Should().NotBeNull();

            instance.Status.Name.Should().Be("Vacant");
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

            DbContext.Database.Instance.AddCommandBehavior(instance.Units.GetSql(), Units.Where(x => x.RealEstatePropertyID == 1));

            instance.ID = 1;

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
