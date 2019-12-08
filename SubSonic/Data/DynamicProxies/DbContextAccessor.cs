using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ext = SubSonic.Extensions;

namespace SubSonic.Data.DynamicProxies
{
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
            string[] keys = Ext.GetForeignKeyName(info);
            object[] keyData = GetKeyData(entity, keys);

            return dbContext.Set<TProperty>().FindByID(keyData).Single();
        }

        public bool IsForeignKeyPropertySetToDefaultValue<TEntity>(TEntity entity, PropertyInfo info)
        {
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
            string[] 
                keys = dbContext.Model.GetEntityModel<TProperty>().PrimaryKey,
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

        public ICollection<TProperty> LoadCollection<TEntity, TProperty>(TEntity entity, PropertyInfo info)
            where TEntity : class
            where TProperty : class
        {
            string[] keys = dbContext.Model.GetEntityModel<TProperty>().PrimaryKey;
            object[] keyData = GetKeyData(entity, keys);

            return dbContext
                .Set<TProperty>()
                .FindByID(keyData)
                .Load()
                .ToHashSet();
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
