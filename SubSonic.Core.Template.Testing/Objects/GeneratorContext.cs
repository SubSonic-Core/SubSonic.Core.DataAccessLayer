using SubSonic.Extensions.SqlServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Template.Testing.Objects
{
    public class GeneratorContext
        : SubSonicContext
    {
        private readonly string connection;

        public GeneratorContext(string connection)
        {
            this.connection = connection;
        }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            builder
                .ConfigureServiceCollection()
                .UseSqlClient((config, options) =>
                {
                    config.ConnectionString = connection;
                });
        }
    }
}
