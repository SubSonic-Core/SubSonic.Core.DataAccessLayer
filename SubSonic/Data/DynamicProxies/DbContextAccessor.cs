using System;
using System.Reflection;
using Ext = SubSonic.SubSonicExtensions;

namespace SubSonic.Data.DynamicProxies
{
    using Linq;
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// this class is referenced in the proxy classes that are generated and will not count references at design time
    /// </remarks>
    internal class DbContextAccessor
    {
        private readonly DbContext dbContext;

        public DbContextAccessor(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public TProperty LoadProperty<TEntity, TProperty>(TEntity entity, PropertyInfo info) 
            where TEntity : class
            where TProperty : class
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            string[] keys = Ext.GetForeignKeyName(info);
            object[] keyData = GetKeyData(entity, keys);

            return dbContext.Set<TProperty>().FindByID(keyData).Single();
        }

        public bool IsForeignKeyPropertySetToDefaultValue<TEntity>(TEntity entity, PropertyInfo info)
            where TEntity : class
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            bool result = false;

            string[] keys = Ext.GetForeignKeyName(info);

            foreach(object value in GetKeyData(entity, keys))
            {
                result |= value.IsDefaultValue(value.GetType());
            }

            return result;
        }

        public void SetForeignKeyProperty<TEntity, TProperty>(TEntity entity, PropertyInfo info)
            where TEntity : class
            where TProperty : class
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            string[] 
                keys = dbContext.Model.GetEntityModel<TProperty>()
                    .GetPrimaryKey()
                    .ToArray(),
                foreignKeys = Ext.GetForeignKeyName(info);
            TProperty property = info.GetValue<TProperty>(entity);

            for(int i = 0; i < keys.Length; i++)
            {
                PropertyInfo 
                    primaryKeyInfo = typeof(TProperty).GetProperty(keys[i]),
                    foriegnKeyInfo = typeof(TEntity).GetProperty(foreignKeys[i]);

                foriegnKeyInfo.SetValue(entity, primaryKeyInfo.GetValue(property), null);
            }
        }

        public System.Linq.IQueryable<TProperty> LoadCollection<TEntity, TProperty>(TEntity entity, PropertyInfo info)
            where TEntity : class
            where TProperty : class
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            string[] 
                keys = dbContext.Model
                    .GetEntityModel<TProperty>().GetPrimaryKey().ToArray(),
                foreignKeys = dbContext.Model
                    .GetRelationshipMapping<TEntity, TProperty>().GetForeignKeys().ToArray();
            object[] keyData = GetKeyData(entity, keys);

            return dbContext
                .Set<TProperty>()
                .FindByID(keyData, foreignKeys);
        }

        private object[] GetKeyData<TEntity>(TEntity entity, string[] keys)
        {
            return typeof(TEntity).GetProperties()
                    .Where(property => keys.Any(key => key.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(property => property.GetValue(entity, null))
                    .ToArray();
        }
    }
}
