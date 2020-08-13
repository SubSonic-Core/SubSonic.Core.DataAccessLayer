using Microsoft.Extensions.Logging;
using SubSonic.Extensions.SqlServer;
using SubSonic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SubSonic.CodeGenerator
{
    public class GeneratorContext
        : SubSonicContext
    {
        private readonly LogLevel logLevel;
        private readonly string connection;

        public GeneratorContext(string connection)
            : this(connection, LogLevel.Debug) { }

        public GeneratorContext(string connection, LogLevel logLevel)
        {
            this.logLevel = logLevel;
            this.connection = connection;
        }

        public ISubSonicSetCollection<Models.Table> Tables { get; private set; }
        public ISubSonicSetCollection<Models.Relationship> Relationships { get; set; }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            builder
                .ConfigureServiceCollection()
                .AddLogging((config) => {
                    config
                    .ClearProviders()
                    .AddDebugLogger<GeneratorContext>()
                    .AddTraceLogger<GeneratorContext>()
                    .SetMinimumLevel(logLevel);
                })
                .UseSqlClient((config, options) =>
                {
                    config.ConnectionString = connection;
                });
        }

        protected override void OnDbModeling(DbModelBuilder builder)
        {
            builder.AddEntityModel<Models.Table>();
            builder.AddEntityModel<Models.Relationship>();
        }
    }
}
