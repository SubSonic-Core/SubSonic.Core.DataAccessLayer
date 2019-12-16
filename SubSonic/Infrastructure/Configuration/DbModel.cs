using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;

    public class DbModel
    {
        private readonly DbContext dbContext;

        public DbModel(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            EntityModels = new List<DbEntityModel>();
        }

        public ICollection<DbEntityModel> EntityModels { get; }

        public IDbEntityModel GetEntityModel<TEntity>()
        {
            return GetEntityModel(typeof(TEntity));
        }

        public IDbEntityModel GetEntityModel(Type entityModelType)
        {
            return EntityModels
                .SingleOrDefault(model => model.EntityModelType == entityModelType)
                .IsNullThrow(new EntityNotRegisteredWithDbModelException(entityModelType));
        }

        public IDbRelationshipMap GetRelationshipMapping<TEntity, TProperty>()
        {
            return GetEntityModel<TEntity>().GetRelationshipWith(GetEntityModel<TProperty>());
        }
    }
}
