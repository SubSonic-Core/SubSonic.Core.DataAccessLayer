﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Ext = SubSonic.SubSonicExtensions;

namespace SubSonic.Data.DynamicProxies
{
    using Linq;
    using Infrastructure;
    using Infrastructure.Schema;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// this class is referenced in the proxy classes that are generated and will not count references at design time
    /// </remarks>
    internal class DbContextAccessor
    {
        public DbContextAccessor(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private DbContext DbContext { get; }

        public DbSchemaModel Model => DbContext.Model;

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

            return DbContext.Set<TProperty>().FindByID(keyData);
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
                keys = DbContext.Model.GetEntityModel<TProperty>()
                    .GetPrimaryKey()
                    .ToArray(),
                foreignKeys = Ext.GetForeignKeyName(info);
            TProperty property = info.GetValue<TProperty>(entity);

            if (!(property is null))
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    PropertyInfo
                        primaryKeyInfo = typeof(TProperty).GetProperty(keys[i]),
                        foriegnKeyInfo = typeof(TEntity).GetProperty(foreignKeys[i]);

                    foriegnKeyInfo.SetValue(entity, primaryKeyInfo.GetValue(property), null);
                }

                OnPropertyChanged(entity);
            }
        }

        public void OnPropertyChanged<TEntity>(TEntity entity)
        {
            if (entity is IEntityProxy proxy)
            {
                proxy.IsDirty = true;
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
                keys = DbContext.Model
                    .GetEntityModel<TEntity>().GetPrimaryKey().ToArray(),
                foreignKeys = DbContext.Model
                    .GetRelationshipMapping<TEntity, TProperty>().GetForeignKeys().ToArray();
            object[] keyData = GetKeyData(entity, keys);

            return DbContext
                .Set<TProperty>()
                .FindByID(keyData, foreignKeys)
                .Load();
        }

        public object[] GetKeyData<TEntity>(TEntity entity, string[] keys)
        {
            return typeof(TEntity).GetProperties()
                    .Where(property => keys.Any(key => key.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(property => property.GetValue(entity, null))
                    .ToArray();
        }
    }
}
