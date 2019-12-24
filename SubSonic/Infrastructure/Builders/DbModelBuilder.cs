using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Ext = SubSonic.SubSonicExtensions;

namespace SubSonic.Infrastructure
{
    using Linq;
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

            foreach(DbCommandQueryAttribute command in entityModelType.GetCustomAttributes<DbCommandQueryAttribute>())
            {
                entity.Commands[command.QueryType] = new DbCommandQuery(command.QueryType, command.StoredProcedureType);
            }

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
                    IsNullable = info.PropertyType.IsNullableType(),
                    IsAutoIncrement = info.GetCustomAttribute<DatabaseGeneratedAttribute>().IsNotNull(x => x.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity),
                    IsComputed = info.GetCustomAttribute<DatabaseGeneratedAttribute>().IsNotNull(x => x.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed),
                    DbType = info.PropertyType.GetDbType()
                };

                entity.Properties.Add(property);
            }

            model.EntityModels.Add(entity);

            model.GenerateProxy<TEntity>();

            return this;
        }

        public DbModelBuilder AddRelationshipFor<TEntity>(Func<IDbNavigationPropertyBuilder> relationship)
            where TEntity : class
        {
            if (relationship is null)
            {
                throw new ArgumentNullException(nameof(relationship));
            }

            IDbEntityModel entity = model.GetEntityModel<TEntity>();

            entity.RelationshipMaps.Add(relationship().RelationshipMap);

            return this;
        }

        public DbRelationshipBuilder<TEntity> GetRelationshipFor<TEntity>() where TEntity : class
        {
            return new DbRelationshipBuilder<TEntity>(model.GetEntityModel<TEntity>());
        }
    }
}
