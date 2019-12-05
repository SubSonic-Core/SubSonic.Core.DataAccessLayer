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

        public TProperty Load<TEntity, TProperty>(TEntity source, PropertyInfo propertyInfo) 
            where TEntity : class
            where TProperty : class
        {
            int id = typeof(TEntity).GetProperty(propertyInfo.GetForeignKeyName())
                .IsNullThrow(new InvalidOperationException())
                .GetValue<int>(source);

            return null;
        }

        public ICollection<TProperty> LoadCollection<TEntity, TProperty>(TEntity source, PropertyInfo propertyInfo)
            where TEntity : class
            where TProperty : class
        {
            int id = typeof(TEntity).GetProperty(source.GetPrimaryKeyName())
                .IsNullThrow(new InvalidOperationException())
                .GetValue<int>(source);

            return new HashSet<TProperty>();
        }
    }
}
