using Microsoft.Extensions.Logging;
using System;

namespace SubSonic.CodeGenerator
{
    public class GeneratorContext
        : SubSonicContext
    {
        protected LogLevel LogLevel { get; }
        protected string ConnectionString { get; }

        protected GeneratorContext(string connection)
            : this(connection, LogLevel.Debug) { }

        protected GeneratorContext(string connection, LogLevel logLevel)
        {
            this.LogLevel = logLevel;
            this.ConnectionString = connection;
        }

        public ISubSonicSetCollection<Models.Table> Tables { get; protected set; }
        public ISubSonicSetCollection<Models.Column> Columns { get; protected set; }
        public ISubSonicSetCollection<Models.Relationship> Relationships { get; protected set; }
        public ISubSonicSetCollection<Models.TableType> TableTypes { get; protected set; }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            builder
                .ConfigureServiceCollection()
                .AddLogging((config) =>
                {
                    config
                    .ClearProviders()
                    .AddDebugLogger<GeneratorContext>()
                    .AddTraceLogger<GeneratorContext>()
                    .SetMinimumLevel(LogLevel);
                });
        }

        protected override void OnDbModeling(DbModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _ = builder
                .AddEntityModel<Models.Table>()
                .AddEntityModel<Models.Column>()
                .AddEntityModel<Models.Relationship>();
        }

        protected override void OnDbModelRelationships(DbModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddRelationshipFor<Models.Table>(() =>
                builder.GetRelationshipFor<Models.Table>()
                    .HasMany(Model => Model.Columns)
                    .WithOne(Model => Model.Table));

            builder.AddRelationshipFor<Models.Table>(() =>
                builder.GetRelationshipFor<Models.Table>()
                    .HasMany(Model => Model.WithOneRelationships)
                    .WithOne(Model => Model.Table));

            builder.AddRelationshipFor<Models.Table>(() =>
                builder.GetRelationshipFor<Models.Table>()
                    .HasMany(Model => Model.WithManyRelationships)
                    .WithOne(Model => Model.ForiegnTable));

            builder.AddRelationshipFor<Models.Column>(() =>
                builder.GetRelationshipFor<Models.Column>()
                    .HasOne(Model => Model.Table)
                    .WithMany(Model => Model.Columns));

            builder.AddRelationshipFor<Models.Relationship>(() =>
                builder.GetRelationshipFor<Models.Relationship>()
                    .HasOne(Model => Model.Table)
                    .WithMany(Model => Model.WithOneRelationships));

            builder.AddRelationshipFor<Models.Relationship>(() =>
                builder.GetRelationshipFor<Models.Relationship>()
                    .HasOne(Model => Model.ForiegnTable)
                    .WithMany(Model => Model.WithOneRelationships));
        }
    }
}
