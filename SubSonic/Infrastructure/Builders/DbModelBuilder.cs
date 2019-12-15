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

            var TableAttr = entityModelType.GetCustomAttribute<TableAttribute>();

            DbEntityModel entity = new DbEntityModel()
            {
                EntityModelType = entityModelType,
                Name = TableAttr.IsNotNull(Table => Table.Name, entityModelType.Name),
                SchemaName = TableAttr.IsNotNull(Table => Table.Schema).IsNull(SubSonicDefaults.SchemaName)
            };

            entity.SetPrimaryKey(Ext.GetPrimaryKeyName<TEntity>());

            foreach (PropertyInfo info in entityModelType.GetProperties())
            {
                var ColumnAttr = info.GetCustomAttribute<ColumnAttribute>();

                DbEntityProperty property = new DbEntityProperty(entity, ColumnAttr.IsNotNull(Column => Column.Name, info.Name))
                {
                    PropertyName = info.Name,
                    SchemaName = entity.QualifiedName,
                    PropertyType = info.PropertyType,
                    IsPrimaryKey = info.GetCustomAttribute<KeyAttribute>().IsNotNull(),
                    IsRequired = info.GetCustomAttribute<RequiredAttribute>().IsNotNull() || !info.PropertyType.IsNullableType(),
                    Size = info.GetCustomAttribute<MaxLengthAttribute>().IsNotNull(Max => Max.Length),
                    Scale = info.PropertyType.IsOfType<decimal>() ? 18 : 0,
                    Precision = info.PropertyType.IsOfType<decimal>() ? 2 : 0,
                    DbType = info.PropertyType.GetDbType()
                };

                entity.Properties.Add(property);
            }



            model.EntityModels.Add(entity);

            return this;
        }
    }
}
