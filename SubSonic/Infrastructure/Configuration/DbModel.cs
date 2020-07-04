using System;
using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Infrastructure
{
    using Data.DynamicProxies;
    using Linq;
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

        internal void GenerateProxy<TEntity>()
        {
            if (dbContext.Options.EnableProxyGeneration)
            {
                DynamicProxy.GetProxyWrapper<TEntity>(dbContext);
            }
        }

        public bool IsEntityModelRegistered<TEntity>()
        {
            return TryGetEntityModel<TEntity>(out _);
        }

        public bool IsEntityModelRegistered(Type entityType)
        {
            return TryGetEntityModel(entityType, out _);
        }

        public bool TryGetEntityModel<TEntity>(out IDbEntityModel model)
        {
            return TryGetEntityModel(typeof(TEntity), out model);
        }

        public IDbEntityModel GetEntityModel<TEntity>()
        {
            return GetEntityModel(typeof(TEntity));
        }

        public bool TryGetEntityModel(Type entityModelType, out IDbEntityModel model)
        {
            model = null;

            try
            {
                model = GetEntityModel(entityModelType);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }

            return model.IsNotNull();
        }

        public IDbEntityModel GetEntityModel(Type entityModelType)
        {
            if (entityModelType.IsNotNull())
            {
                return EntityModels
                    .SingleOrDefault(model => model.EntityModelType == entityModelType || model.EntityModelType == entityModelType.BaseType)
                    .IsNullThrow(new EntityNotRegisteredWithDbModelException(entityModelType));
            }
            return null;
        }

        public IDbRelationshipMap GetRelationshipMapping<TEntity, TProperty>()
        {
            return GetEntityModel<TEntity>().GetRelationshipWith(GetEntityModel<TProperty>());
        }
    }
}
