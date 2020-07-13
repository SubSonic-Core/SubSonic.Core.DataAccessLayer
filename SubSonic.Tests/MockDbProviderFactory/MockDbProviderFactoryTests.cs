using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.Data.Builders;
using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.MockDbClient.Syntax;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace SubSonic.Tests.MockDbProviderFactory
{
    [TestFixture]
    public partial class MockDbProviderFactoryTests
    {
        private const string InstanceFieldName = "Instance";

        private MockDbClientFactory Factory => MockDbClientFactory.Instance;

        [SetUp]
        public void SetUp()
        {
            Factory.ClearBehaviors();
        }

        [Test]
        public void CanGetTheInstanceFieldOfMockDbProviderFactory()
        {
            Type providerFactoryType = typeof(MockDbClientFactory);

            FieldInfo fieldInstance = providerFactoryType.GetField(InstanceFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);

            fieldInstance.Should().NotBeNull();
            fieldInstance.FieldType.Should().BeDerivedFrom<DbProviderFactory>();

            fieldInstance.GetValue(null).Should().NotBeNull();
        }

        [Test]
        public void CanGetTheInstanceFieldOfSubSonicFactoryWrapper()
        {
            Type providerFactoryType = typeof(SubSonicMockDbClient);

            FieldInfo fieldInstance = providerFactoryType.GetField(InstanceFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);

            fieldInstance.Should().NotBeNull();
            fieldInstance.FieldType.Should().BeDerivedFrom<DbProviderFactory>();

            fieldInstance.GetValue(null).Should().NotBeNull();
        }

        [Test]
        public void CreatesProviderFromFactory()
        {
            DbProviderFactory factory = null;

#if NETFRAMEWORK
            factory = DbProviderFactories.GetFactory(SetUpMockDb.ProviderInvariantName);
#else
            factory = DbProviderFactories.GetFactory(SetUpMockDb.ProviderInvariantName);
#endif

            factory.Should().BeOfType<MockDbClientFactory>();
        }

        [Test]
        public void CanSetupResultForSimpleForQuery()
        {
            DataTable customers;

            using (DataTableBuilder builder = new DataTableBuilder())
            {
                builder.AddColumn("userid", typeof(int))
                .AddColumn("email", typeof(string))
                .AddRow(1, "a@a.com")
                .AddRow(10, "b@b.com");

                customers = builder.DataTable;
            }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
        public void CanFillDataSet()
        {
            DataTable
                users,
                orders;

            using (var builder = new DataTableBuilder())
            {
                builder
                .AddColumn("customerid", typeof(int))
                .AddColumn("firstname", typeof(string))
                .AddColumn("lastname", typeof(string))
                .AddRow(1, "joe", "black")
                .AddRow(1, "kurt", "vonnegut");

                users = builder.DataTable;
            }

            using (var builder = new DataTableBuilder())
            {
                builder
                .AddColumn("orderid", typeof(int))
                .AddColumn("userid", typeof(int))
                .AddColumn("total", typeof(double))
                .AddRow(100, 1, 10.10)
                .AddRow(101, 1, 10.20)
                .AddRow(202, 2, 20.10)
                .AddRow(203, 2, 20.20);

                orders = builder.DataTable;
            }

            Factory.AddBehavior(new MockCommandBehavior()
                .When(cmd => cmd.CommandText.Contains("from customers"))
                .ReturnsData(users));
            Factory.AddBehavior(new MockCommandBehavior()
                .When(cmd => cmd.CommandText.Contains("from orders"))
                .ReturnsData(orders));

            using (var result = SUT.DataAccess.GetAllOrders())
            {
                Assert.AreEqual(2, result.Tables.Count);
                Assert.AreEqual(2, result.Tables["customers"].Rows.Count);
                Assert.AreEqual(4, result.Tables["orders"].Rows.Count);
            }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
        public void CanCreateTransaction()
        {
            DbProviderFactory factory = null;

#if NETFRAMEWORK
            factory = DbProviderFactories.GetFactory(SetUpMockDb.ProviderInvariantName) as MockDbClientFactory;
#else
            factory = DbProviderFactories.GetFactory(SetUpMockDb.ProviderInvariantName) as MockDbClientFactory;
#endif

            using (var conn = factory.CreateConnection())
            using (var trn = conn.BeginTransaction())
            {
                Assert.IsNotNull(trn);
            }
        }
    }
}
