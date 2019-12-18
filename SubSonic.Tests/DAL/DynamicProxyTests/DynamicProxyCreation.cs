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
WHERE ([{0}].[ID] = 1) <> 0".Format("T1");

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            DbContext.Database.Instance.AddCommandBehavior(expected, Statuses.Where(x => x.ID == 1));

            instance.StatusID = 1;

            instance.Status.Should().NotBeNull();

            instance.Status.Name.Should().Be("Vacant");
        }

        [Test]
        public void CanLazyLoadAnythingFromAnything()
        {
            string
                units =
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
FROM [dbo].[Unit] AS [{0}]",
                property =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[ID] = {1}) <> 0",
                status =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = {1}) <> 0";

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1"), Units);
            DbContext.Database.Instance.AddCommandBehavior(property.Format("T1", 1), RealEstateProperties.Where(x => x.ID == 1));
            DbContext.Database.Instance.AddCommandBehavior(property.Format("T1", 2), RealEstateProperties.Where(x => x.ID == 2));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 1), Statuses.Where(x => x.ID == 1));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 2), Statuses.Where(x => x.ID == 2));
            DbContext.Database.Instance.AddCommandBehavior(status.Format("T1", 3), Statuses.Where(x => x.ID == 3));

            foreach(Unit unit in DbContext.Units)
            {
                ((IEntityProxy)unit).IsNew.Should().BeFalse();

                unit.RealEstateProperty.Should().NotBeNull();
                unit.RealEstateProperty.ID.Should().Be(unit.RealEstatePropertyID);

                ((IEntityProxy)unit.RealEstateProperty).IsNew.Should().BeFalse();

                unit.RealEstateProperty.Status.Should().NotBeNull();
                unit.RealEstateProperty.Status.ID.Should().Be(unit.RealEstateProperty.StatusID);

                ((IEntityProxy)unit.RealEstateProperty.Status).IsNew.Should().BeFalse();
            }

            DbContext.Units.Should().NotBeEmpty();
        }

        [Test]
        public void ProxyCollectionPropertyWillNotBeNullOnGet()
        {
            string
                units =
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = {1}) <> 0";

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            instance.Units = null;

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 0), Units.Where(x => x.RealEstatePropertyID == 0));

            instance.Units.Should().NotBeNull();
            instance.Units.Should().BeEmpty();
        }

        [Test]
        public void ProxyCollectionPropertyWillLoadWhenNotNullAndCountIsZeroOnGet()
        {
            string
                units =
@"SELECT [{0}].[ID], [{0}].[RealEstatePropertyID], [{0}].[StatusID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = {1}) <> 0";

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(DbContext);

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 0), Units.Where(x => x.RealEstatePropertyID == 0));

            instance.Units.Should().NotBeNull();
            // have yet to hit the db
            instance.Units.Should().BeEmpty();

            DbContext.Database.Instance.AddCommandBehavior(units.Format("T1", 1), Units.Where(x => x.RealEstatePropertyID == 1));

            instance.ID = 1;

            instance.Units.Should().NotBeEmpty();
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
