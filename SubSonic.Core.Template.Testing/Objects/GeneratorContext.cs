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
        public ISubSonicSetCollection<Models.Column> Columns { get; private set; }
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
            builder.AddEntityModel<Models.Column>();
            builder.AddEntityModel<Models.Relationship>();
        }

        protected override void OnDbModelRelationships(DbModelBuilder builder)
        {
            builder.AddRelationshipFor<Models.Table>(() =>
                builder.GetRelationshipFor<Models.Table>()
                    .HasMany(Model => Model.Columns)
                    .WithOne(Model => Model.Table));

            builder.AddRelationshipFor<Models.Table>(() =>
                builder.GetRelationshipFor<Models.Table>()
                    .HasMany(Model => Model.Relationships)
                    .WithOne(Model => Model.Table));

            builder.AddRelationshipFor<Models.Column>(() =>
                builder.GetRelationshipFor<Models.Column>()
                    .HasOne(Model => Model.Table)
                    .WithMany(Model => Model.Columns));

            builder.AddRelationshipFor<Models.Relationship>(() =>
                builder.GetRelationshipFor<Models.Relationship>()
                    .HasOne(Model => Model.Table)
                    .WithMany(Model => Model.Relationships));

            builder.AddRelationshipFor<Models.Relationship>(() =>
                builder.GetRelationshipFor<Models.Relationship>()
                    .HasOne(Model => Model.ForiegnTable)
                    .WithMany(Model => Model.Relationships));
        }
    }
}
