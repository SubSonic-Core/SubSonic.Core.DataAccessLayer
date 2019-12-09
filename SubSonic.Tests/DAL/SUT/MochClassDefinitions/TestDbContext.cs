using Microsoft.Extensions.Logging;
using SubSonic.Extensions.Test;
using SubSonic.Infrastructure;
using SubSonic.Tests.DAL.SUT.NUnit;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.SUT
{
    public class TestDbContext
        : DbContext
    {
        public TestDbContext()
            : base()
        {

        }

        public DbSetCollection<Models.RealEstateProperty> RealEstateProperties { get; private set; }

        public DbSetCollection<Models.Status> Statuses { get; private set; }

        public DbSetCollection<Models.Unit> Units { get; private set; }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnDbConfiguring(builder);

            builder
                .ConfigureServiceCollection()
                .AddLogging((config) => config.AddNUnitLogger<TestDbContext>(LogLevel.Debug))
                .UseMockDbProviderFactory()
                .EnableProxyGeneration();
        }

        protected override void OnDbModeling(DbModelBuilder builder)
        {
            builder
                .AddEntityModel<Models.RealEstateProperty>()
                .AddEntityModel<Models.Status>()
                .AddEntityModel<Models.Unit>();
        }
    }
}
