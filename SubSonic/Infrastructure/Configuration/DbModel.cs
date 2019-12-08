using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbModel
    {
        private readonly DbContext dbContext;

        public DbModel(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            EntityModels = new List<DbEntityModel>();
        }

        public ICollection<DbEntityModel> EntityModels { get; }

        public DbEntityModel GetEntityModel<TEntity>()
        {
            return GetEntityModel(typeof(TEntity));
        }

        public DbEntityModel GetEntityModel(Type entityModelType)
        {
            return EntityModels
                .SingleOrDefault(model => model.EntityModelType == entityModelType)
                .IsNullThrow(new EntityNotRegisteredWithDbModelException(entityModelType));
        }
    }
}
