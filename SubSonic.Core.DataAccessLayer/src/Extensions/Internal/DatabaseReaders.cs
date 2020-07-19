using SubSonic.Data.DynamicProxies;
using SubSonic;
using SubSonic.Schema;
using System;
using System.Data;
using System.Reflection;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static object LoadInstanceOf(this IDataRecord data, object entity)
        {
            Type entityType = entity.GetType();
            SubSonicContext.DbModel.TryGetEntityModel(entityType, out IDbEntityModel model);

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (model != null && model[property.Name].EntityPropertyType != DbEntityPropertyType.Value)
                {
                    continue;
                }

                object value = data[model?[property.Name].Name ?? property.Name];

                if (property.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    entityType
                        .GetProperty(property.Name)
                        .SetValue(entity, value);
                }
            }

            if (entity is IEntityProxy)
            {
                ((IEntityProxy)entity).IsNew = false;
                ((IEntityProxy)entity).IsDirty = false;
            }

            return entity;
        }
        public static object ActivateAndLoadInstanceOf(this IDataRecord data, Type entityType)
        {
            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            object entity = null;

            if (SubSonicContext.DbOptions.EnableProxyGeneration &&
                SubSonicContext.DbModel.IsEntityModelRegistered(entityType))
            {
                DynamicProxyWrapper wrapper = DynamicProxy.GetProxyWrapper(entityType);

                entity = Activator.CreateInstance(wrapper.Type, SubSonicContext.ServiceProvider.GetService<DbContextAccessor>());
            }
            else
            {
                entity = Activator.CreateInstance(entityType);
            }

            return data.LoadInstanceOf(entity);
        }
    }
}
