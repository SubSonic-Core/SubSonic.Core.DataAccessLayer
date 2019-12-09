using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using SubSonic.Extensions.Test.Models;
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
                .AddEntityModel<RealEstateProperty>()
                .AddEntityModel<Status>()
                .AddEntityModel<Unit>();
        }
    }
}
