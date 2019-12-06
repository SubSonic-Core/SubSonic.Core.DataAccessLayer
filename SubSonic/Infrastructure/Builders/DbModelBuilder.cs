using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Ext = SubSonic.Extensions;

namespace SubSonic.Infrastructure
{
    public class DbModelBuilder
    {
        private readonly DbModel model;

        public DbModelBuilder(DbModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public DbModelBuilder AddEntityModel<TEntity>()
            where TEntity : class
        {
            Type entityModelType = typeof(TEntity);

            DbEntityModel entity = new DbEntityModel()
            {
                EntityModelType = entityModelType
            };

            entity.PrimaryKey = Ext.GetPrimaryKeyName<TEntity>();

            foreach (PropertyInfo info in entityModelType.GetProperties())
            {
                var column = info.GetCustomAttribute<ColumnAttribute>();

                DbEntityProperty property = new DbEntityProperty(column.IsNotNull(col => col.Name, info.Name));

                property.IsKey = info.GetCustomAttribute<KeyAttribute>().IsNotNull();

                entity.Properties.Add(property);
            }



            model.EntityModels.Add(entity);

            return this;
        }
    }
}
