using FluentAssertions;
using NUnit.Framework;
using SubSonic.Data.DynamicProxies;
using SubSonic.Extensions.Test.Models;
using SubSonic.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using Linq;
    using SUT;

    [TestFixture]
    public partial class DynamicProxyTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            string
                units =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]",
                units_by_property =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = @realestatepropertyid_1)",
                property =
@"SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [{0}]
WHERE ([{0}].[ID] = @id_1)",
                renters =
@"SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]
FROM [dbo].[Renter] AS [{0}]
WHERE ([{0}].[UnitID] = @unitid_1)",
                status =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @id_1)";

            Context.Database.Instance.AddCommandBehavior(units.Format("T1"), Units);
            Context.Database.Instance.AddCommandBehavior(units_by_property.Format("T1"), cmd => Units
                .Where(x => x.RealEstatePropertyID == cmd.Parameters["@realestatepropertyid_1"].GetValue<int>())
                .ToDataTable());
            Context.Database.Instance.AddCommandBehavior(property.Format("T1"), cmd => RealEstateProperties
                .Where(x =>
                    x.ID == cmd.Parameters["@id_1"].GetValue<int>())
                .ToDataTable());
            Context.Database.Instance.AddCommandBehavior(renters.Format("T1"), cmd => Renters
                .Where(x =>
                    x.UnitID == cmd.Parameters["@unitid_1"].GetValue<int>())
                .ToDataTable());
            Context.Database.Instance.AddCommandBehavior(status.Format("T1"), cmd => Statuses
                .Where(x =>
                    x.ID == cmd.Parameters["@id_1"].GetValue<int>())
                .ToDataTable());
        }

        [Test]
        public void BuildProxyForElegibleType()
        {
            DynamicProxyWrapper proxyWrapper = DynamicProxy.GetProxyWrapper<RealEstateProperty>(Context);

            proxyWrapper.IsElegibleForProxy.Should().BeTrue();
            proxyWrapper.Type.Should().BeDerivedFrom<RealEstateProperty>();

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            instance.Should().BeAssignableTo<RealEstateProperty>();
        }

        [Test]
        public void DynamicProxyImplementsIEntityProxy()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            ((IEntityProxy)instance).Should().NotBeNull();

            ((IEntityProxy)instance).KeyData.Should().NotBeEmpty();
            ((IEntityProxy)instance).KeyData.Should().BeEquivalentTo(new object[] { 0 });

            ((IEntityProxy)instance).IsNew.Should().BeTrue();
            ((IEntityProxy)instance).IsNew = false;
            ((IEntityProxy)instance).IsNew.Should().BeFalse();

            ((IEntityProxy)instance).SetKeyData(new object[] { 1 });
            ((IEntityProxy)instance).KeyData.Should().BeEquivalentTo(new object[] { 1 });

            ((IEntityProxy<RealEstateProperty>)instance).Data.Should().BeSameAs(instance);
        }

        [Test]
        public void ProxyNavigationPropertyWillSetForeignKeysOnSet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            instance.StatusID.Should().Be(0);

            instance.Status = new Status() { ID = 1, Name = "Available" };

            instance.StatusID.Should().Be(1);

            ((IEntityProxy)instance).IsDirty.Should().BeTrue();

            Renter renter = DynamicProxy.CreateProxyInstanceOf<Renter>(Context);

            renter.PersonID.Should().Be(0);

            renter.Person = new Person() { ID = 1 };

            renter.PersonID.Should().Be(1);

            ((IEntityProxy)renter).IsDirty.Should().BeTrue();
        }

        [Test]
        public void ProxyNavigationPropertyWillNotLoadWhenNullAndForiengKeyIsDefaultValueOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            instance.Status.Should().BeNull();
        }

        [Test]
        public void ProxyNavigationPropertyWillLoadWhenNullAndForiengKeyIsNotDefaultValueOnGet()
        {
            string expected =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = 1)".Format("T1");

            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            Context.Database.Instance.AddCommandBehavior(expected, Statuses.Where(x => x.ID == 1));

            instance.StatusID = 1;

            instance.Status.Should().NotBeNull();

            instance.Status.Name.Should().Be("Vacant");
        }

        [Test]
        public void CanLazyLoadAnythingFromAnything()
        {
            foreach (Unit unit in Context.Units)
            {
                ((IEntityProxy)unit).IsNew.Should().BeFalse();

                unit.RealEstateProperty.Should().NotBeNull();
                unit.RealEstateProperty.ID.Should().Be(unit.RealEstatePropertyID);

                ((IEntityProxy)unit.RealEstateProperty).IsNew.Should().BeFalse();

                unit.RealEstateProperty.Status.Should().NotBeNull();
                unit.RealEstateProperty.Status.ID.Should().Be(unit.RealEstateProperty.StatusID);

                foreach (Renter renter in unit.Renters)
                {
                    ((IEntityProxy)renter).IsNew.Should().BeFalse();
                }

                ((IEntityProxy)unit.RealEstateProperty.Status).IsNew.Should().BeFalse();
            }

            Context.Units.Should().NotBeEmpty();
        }

        [Test]
        public void ProxyCollectionPropertyWillNotBeNullOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            instance.Units = null;

            instance.Units.Should().NotBeNull();
            instance.Units.Should().BeEmpty();
        }

        [Test]
        public void ProxyCollectionPropertyWillLoadWhenNotNullAndCountIsZeroOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            instance.Units.Should().NotBeNull();
            // have yet to hit the db
            instance.Units.Should().BeEmpty();

            instance.ID = 1;

            instance.Units.Should().NotBeEmpty();
        }

        [Test]
        public void ProxyCollectionPropertyWillNotLoadWhenNotNullAndCountGreaterThanZeroOnGet()
        {
            RealEstateProperty instance = DynamicProxy.CreateProxyInstanceOf<RealEstateProperty>(Context);

            instance.Units = new HashSet<Unit>(new[] { DynamicProxy.CreateProxyInstanceOf<Unit>(Context) });

            instance.Units.Should().NotBeNull();
        }
    }
}
