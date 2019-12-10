using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Ext = SubSonic.SubSonicExtensions;

namespace SubSonic.Infrastructure
{
    using Schema;

    public class DbModelBuilder
    {
        private readonly DbModel model;

        public DbModelBuilder(DbModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// TODO::Requires follow up
        public DbModelBuilder AddEntityModel<TEntity>()
            where TEntity : class
        {
            Type entityModelType = typeof(TEntity);

            DbEntityModel entity = new DbEntityModel()
            {
                EntityModelType = entityModelType
            };

            entity.SetPrimaryKey(Ext.GetPrimaryKeyName<TEntity>());

            foreach (PropertyInfo info in entityModelType.GetProperties())
            {
                var column = info.GetCustomAttribute<ColumnAttribute>();

                DbEntityProperty property = new DbEntityProperty(entity, column.IsNotNull(col => col.Name, info.Name))
                {
                    PropertyName = info.Name,
                    PropertyType = info.PropertyType,
                    IsPrimaryKey = info.GetCustomAttribute<KeyAttribute>().IsNotNull(),
                    IsRequired = info.GetCustomAttribute<RequiredAttribute>().IsNotNull() || !info.PropertyType.IsNullableType()
                };

                entity.Properties.Add(property);
            }



            model.EntityModels.Add(entity);

            return this;
        }
    }
}
