using SubSonic.Data.DynamicProxies;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static object LoadInstanceOf(this IDataRecord data, object entity)
        {
            IDbEntityModel model = DbContext.DbModel.GetEntityModel(entity.GetType());

            foreach (IDbEntityProperty property in model.Properties)
            {
                if (property.EntityPropertyType == DbEntityPropertyType.Value)
                {
                    model.EntityModelType
                        .GetProperty(property.PropertyName)
                        .SetValue(entity, data[property.Name]);
                }
            }

            if (entity is IEntityProxy)
            {
                ((IEntityProxy)entity).IsNew = false;
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

            if (DbContext.DbOptions.EnableProxyGeneration)
            {
                DynamicProxyWrapper wrapper = DynamicProxy.GetProxyWrapper(entityType);

                entity = Activator.CreateInstance(wrapper.Type, DbContext.ServiceProvider.GetService<DbContextAccessor>());
            }
            else
            {
                entity = Activator.CreateInstance(entityType);
            }

            return data.LoadInstanceOf(entity);
        }
    }
}
