using SubSonic.Extensions.SqlServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.CodeGenerator
{
    public class GeneratorContext
        : SubSonicContext
    {
        private readonly string connection;

        public GeneratorContext(string connection)
        {
            this.connection = connection;
        }

        public ISubSonicSetCollection<Models.Table> Tables { get; private set; }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            builder
                .ConfigureServiceCollection()
                .UseSqlClient((config, options) =>
                {
                    config.ConnectionString = connection;
                });
        }

        protected override void OnDbModeling(DbModelBuilder builder)
        {
            builder.AddEntityModel<Models.Table>();
        }
    }
}
