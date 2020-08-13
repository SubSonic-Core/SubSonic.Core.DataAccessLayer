using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Ext = SubSonic.SubSonicExtensions;

namespace SubSonic
{ 
    using src;
    using Linq;
    using Schema;

    public class DbModelBuilder
    {
        private readonly SubSonicSchemaModel model;

        internal static class DataAccessProperties
        {
            public const string CorrelationID = "CorrelationID";
        }

        public DbModelBuilder(SubSonicSchemaModel model)
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
            var TableTypeAttr = entityModelType.GetCustomAttribute<DbUserDefinedTableTypeAttribute>();

            DbEntityModel entity = new DbEntityModel()
            {
                EntityModelType = entityModelType,
                Name = TableAttr.IsNotNull(Table => Table.Name, entityModelType.Name),
                SchemaName = TableAttr.IsNotNull(Table => Table.Schema).IsNull(SubSonicDefaults.SchemaName),
                DefinedTableType = TableTypeAttr,
                DbObjectType = DbObjectTypeEnum.Table
            };

            if (entityModelType.GetCustomAttribute<Attributes.DbViewAttribute>() is Attributes.DbViewAttribute ViewAttr)
            {
                entity.Name = ViewAttr.IsNotNull(Table => Table.Name, entityModelType.Name);
                entity.SchemaName = ViewAttr.IsNotNull(Table => Table.Schema).IsNull(SubSonicDefaults.SchemaName);
                entity.DbObjectType = DbObjectTypeEnum.View;
            }

            foreach (DbCommandQueryAttribute command in entityModelType.GetCustomAttributes<DbCommandQueryAttribute>())
            {
                entity.Commands[command.QueryType] = new DbCommandQuery(command.QueryType, command.StoredProcedureType);
            }

            entity.SetPrimaryKey(Ext.GetPrimaryKeyName<TEntity>());

            foreach (PropertyInfo info in entityModelType.GetProperties())
            {
                var ColumnAttr = info.GetCustomAttribute<ColumnAttribute>();

                DatabaseGeneratedAttribute databaseGenerated = info.GetCustomAttribute<DatabaseGeneratedAttribute>();

                bool
                    isComputedField = databaseGenerated.IsNotNull(x => x.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed),
                    isAutoIncrement = databaseGenerated.IsNotNull(x => x.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity),
                    isRequired      = info.GetCustomAttribute<RequiredAttribute>().IsNotNull(),
                    isReadOnly      = info.CanRead && (isComputedField || isAutoIncrement || !info.CanWrite),
                    isNullableType  = info.PropertyType.IsNullableType() || info.PropertyType == typeof(string);

                DbEntityProperty property = new DbEntityProperty(entity, ColumnAttr.IsNotNull(Column => Column.Name, info.Name))
                {
                    PropertyName = info.Name,
                    SchemaName = entity.QualifiedName,
                    PropertyType = info.PropertyType,
                    IsPrimaryKey = info.GetCustomAttribute<KeyAttribute>().IsNotNull(),
                    IsRequired = isRequired || !(isNullableType),
                    Size = info.GetCustomAttribute<MaxLengthAttribute>().IsNotNull(Max => Max.Length),
                    Scale = info.PropertyType.IsOfType<decimal>() ? 18 : 0,
                    Precision = info.PropertyType.IsOfType<decimal>() ? 2 : 0,
                    IsNullable = !isRequired && isNullableType,
                    IsAutoIncrement = isAutoIncrement,
                    IsComputed = isComputedField,
                    IsReadOnly = isReadOnly,
                    DbType = info.PropertyType.GetDbType()
                };

                if (property.EntityPropertyType == DbEntityPropertyType.Navigation)
                {
                    property.ForeignKeys = Ext.GetForeignKeyName(info);
                }

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
