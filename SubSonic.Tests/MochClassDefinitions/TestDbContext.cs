using System;
using System.Collections.Generic;
using System.Text;
using SubSonic.Extensions.Test;
using SubSonic.Infrastructure;
using Models = SubSonic.Test.Rigging.Models;

namespace SubSonic.Tests
{
    public class TestDbContext
        : DbContext
    {
        public TestDbContext()
            : base()
        {

        }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnDbConfiguring(builder);

            builder
                .ConfigureServiceCollection()
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
