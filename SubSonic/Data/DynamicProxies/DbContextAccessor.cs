using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Data.DynamicProxies
{
    internal class DbContextAccessor
    {
        private readonly DbContext dbContext;

        public DbContextAccessor(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public TProperty Load<TEntity, TProperty>(TEntity entity, PropertyInfo info) 
            where TEntity : class
            where TProperty : class
        {
            int id = typeof(TEntity).GetProperty(info.GetForeignKeyName())
                .IsNullThrow(new InvalidOperationException())
                .GetValue<int>(entity);

            return null;
        }

        public void SetForeignKey<TEntity>(TEntity entity, PropertyInfo info)
        {
            throw new NotImplementedException();
        }

        public ICollection<TProperty> LoadCollection<TEntity, TProperty>(TEntity entity, PropertyInfo info)
            where TEntity : class
            where TProperty : class
        {
            int id = typeof(TEntity).GetProperty(entity.GetPrimaryKeyName())
                .IsNullThrow(new InvalidOperationException())
                .GetValue<int>(entity);

            return new HashSet<TProperty>();
        }
    }
}
