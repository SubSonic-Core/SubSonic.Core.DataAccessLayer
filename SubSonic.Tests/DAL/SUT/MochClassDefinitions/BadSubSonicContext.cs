using SubSonic.Extensions;
using SubSonic.Extensions.Test;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.SUT
{
    public class BadSubSonicContext
        : SubSonic.SubSonicContext
    {
        public BadSubSonicContext()
            : base()
        {

        }

        public ISubSonicSetCollection<Models.RealEstateProperty> RealEstateProperties { get; private set; }

        public ISubSonicSetCollection<Models.Status> Statuses { get; private set; }

        public ISubSonicSetCollection<Models.Unit> Units { get; private set; }

        public ISubSonicSetCollection<Models.Renter> Renters { get; private set; }

        public ISubSonicSetCollection<Models.Person> People { get; private set; }

        protected override void OnDbConfiguring(DbContextOptionsBuilder config)
        {
            config.UseMockDbClient((builder, options) =>
                {
                    builder
                        .SetDatasource("localhost")
                        .SetInitialCatalog("test")
                        .SetIntegratedSecurity(true);
                });
        }

        protected override void OnDbModeling(DbModelBuilder builder)
        {
            builder.AddEntityModel<Models.RealEstateProperty>();
            builder.AddEntityModel<Models.Status>();
            builder.AddEntityModel<Models.Unit>();
            builder.AddEntityModel<Models.Renter>();
            builder.AddEntityModel<Models.Person>();

            builder.AddRelationshipFor<Models.RealEstateProperty>(() =>
                builder.GetRelationshipFor<Models.RealEstateProperty>()
                    .HasMany(Model => Model.Units)
                    .WithOne(Model => Model.RealEstateProperty));

            builder.AddRelationshipFor<Models.RealEstateProperty>(() =>
                builder.GetRelationshipFor<Models.RealEstateProperty>()
                    .HasOne(Model => Model.Status)
                    .WithNone());

            builder.AddRelationshipFor<Models.Unit>(() =>
                builder.GetRelationshipFor<Models.Unit>()
                    .HasOne(Model => Model.RealEstateProperty)
                    .WithMany(Model => Model.Units));

            builder.AddRelationshipFor<Models.Unit>(() =>
                builder.GetRelationshipFor<Models.Unit>()
                    .HasMany(Model => Model.Renters)
                    .WithOne(Model => Model.Unit));

            builder.AddRelationshipFor<Models.Unit>(() =>
                builder.GetRelationshipFor<Models.Unit>()
                    .HasOne(Model => Model.Status)
                    .WithNone());

            builder.AddRelationshipFor<Models.Person>(() =>
                builder.GetRelationshipFor<Models.Person>()
                    .HasMany(Model => Model.Renters)
                    .WithOne(Model => Model.Person));

            builder.AddRelationshipFor<Models.Person>(() =>
                builder.GetRelationshipFor<Models.Person>()
                    .HasMany(Model => Model.Units)
                    .UsingLookup(Model => Model.Renters)
                    .WithMany());
        }
    }
}
