using SubSonic.Data.DynamicProxies;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Data;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static object LoadInstanceOf(this IDataRecord data, object entity)
        {
            IDbEntityModel model = SubSonicContext.DbModel.GetEntityModel(entity.GetType());

            foreach (IDbEntityProperty property in model.Properties)
            {
                if (property.EntityPropertyType == DbEntityPropertyType.Value)
                {
                    object value = data[property.Name];

                    if (property.PropertyType.IsAssignableFrom(value.GetType()))
                    {
                        model.EntityModelType
                            .GetProperty(property.PropertyName)
                            .SetValue(entity, value);
                    }
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

            if (SubSonicContext.DbOptions.EnableProxyGeneration)
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
