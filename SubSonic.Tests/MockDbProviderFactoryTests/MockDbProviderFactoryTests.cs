using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test.MockDbProvider;
using SubSonic.Extensions.Test.MockDbProvider.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Tests.MockDbProviderFactoryTests
{
    [TestFixture]
    public partial class MockDbProviderFactoryTests
    {
        private const string InstanceFieldName = "Instance";

        private MockDbProviderFactory Factory => MockDbProviderFactory.Instance;

        [SetUp]
        public void SetUp()
        {
            Factory.ClearBehaviors();
        }

        [Test]
        public void CanGetTheInstanceFieldOfMockDbProviderFactory()
        {
            Type providerFactoryType = typeof(MockDbProviderFactory);

            FieldInfo fieldInstance = providerFactoryType.GetField(InstanceFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);

            fieldInstance.Should().NotBeNull();
            fieldInstance.FieldType.Should().BeDerivedFrom<DbProviderFactory>();

            fieldInstance.GetValue(null).Should().NotBeNull();
        }

        [Test]
        public void CreatesProviderFromFactory()
        {
            var factory = DbProviderFactories.GetFactory(SetUpMockDb.ProviderInvariantName);

            factory.Should().BeOfType<MockDbProviderFactory>();
        }

        [Test]
        public void CanSetupResultForSimpleForQuery()
        {
            var customers = new DataTableBuilder()
                .AddColumn("userid", typeof(Int32))
                .AddColumn("email", typeof(String))
                .AddRow(1, "a@a.com")
                .AddRow(10, "b@b.com").DataTable;

            var behavior = new MockCommandBehavior()
                .When(c => c.CommandText.StartsWith("select *"))
                .ReturnsData(customers);
            Factory.AddBehavior(behavior);

            var table = SUT.DataAccess.GetAllUsers();
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(1, table.Rows[0][0]);
            Assert.AreEqual("a@a.com", table.Rows[0][1]);
            Assert.AreEqual(10, table.Rows[1][0]);
            Assert.AreEqual("b@b.com", table.Rows[1][1]);
        }

        [Test]
        public void CanFillDataSet()
        {
            var users = new DataTableBuilder()
                .AddColumn("customerid", typeof(Int32))
                .AddColumn("firstname", typeof(String))
                .AddColumn("lastname", typeof(String))
                .AddRow(1, "joe", "black")
                .AddRow(1, "kurt", "vonnegut").DataTable;

            var orders = new DataTableBuilder()
                .AddColumn("orderid", typeof(Int32))
                .AddColumn("userid", typeof(Int32))
                .AddColumn("total", typeof(double))
                .AddRow(100, 1, 10.10)
                .AddRow(101, 1, 10.20)
                .AddRow(202, 2, 20.10)
                .AddRow(203, 2, 20.20).DataTable;

            Factory.AddBehavior(new MockCommandBehavior()
                .When(cmd => cmd.CommandText.Contains("from customers"))
                .ReturnsData(users));
            Factory.AddBehavior(new MockCommandBehavior()
                .When(cmd => cmd.CommandText.Contains("from orders"))
                .ReturnsData(orders));

            var result = SUT.DataAccess.GetAllOrders();

            Assert.AreEqual(2, result.Tables.Count);
            Assert.AreEqual(2, result.Tables["customers"].Rows.Count);
            Assert.AreEqual(4, result.Tables["orders"].Rows.Count);
        }

        [Test]
        public void CanGetScalar()
        {
            Factory.AddBehavior(new MockCommandBehavior()
                .When(cmd => cmd.CommandText.StartsWith("select userid from users"))
                .ReturnsScalar(15559));

            var result = SUT.DataAccess.GetUserId("abc", "test");
            Assert.AreEqual(15559, result);
        }

        [Test]
        public void CanCreateTransaction()
        {
            var factory = DbProviderFactories.GetFactory(SetUpMockDb.ProviderInvariantName) as MockDbProviderFactory;

            using (var conn = factory.CreateConnection())
            {
                using (var trn = conn.BeginTransaction())
                {
                    Assert.IsNotNull(trn);
                }
            }
        }
    }
}
