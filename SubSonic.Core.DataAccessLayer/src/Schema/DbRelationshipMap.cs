using System;
using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Schema
{
    using Linq;
    using SubSonic.Data.Caching;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Reflection;

    public class DbRelationshipMap
        : IDbRelationshipMap
    {
        private readonly string[] foreignKeyNames;

        public DbRelationshipMap(
            DbRelationshipType relationshipType,
            IDbEntityModel lookupModel,
            IDbEntityModel foreignModel, 
            string[] foreignKeyNames,
            bool isReciprocated)
        {
            if((relationshipType == DbRelationshipType.HasManyWithMany) && (lookupModel is null))
            {
                throw new ArgumentNullException(nameof(lookupModel));
            }

            this.foreignKeyNames = foreignKeyNames ?? throw new ArgumentNullException(nameof(foreignKeyNames));
            RelationshipType = relationshipType;
            LookupModel = lookupModel;
            ForeignModel = foreignModel ?? throw new ArgumentNullException(nameof(foreignModel));
            IsReciprocated = isReciprocated;
            
        }

        public bool IsReciprocated { get; }

        public bool IsLookupMapping => !(LookupModel is null);

        public DbRelationshipType RelationshipType { get; }

        public IDbEntityModel LookupModel { get; }

        public IDbEntityModel ForeignModel { get; }

        public IEnumerable<string> GetForeignKeys(IDbEntityModel entityModel)
        {
            if (RelationshipType == DbRelationshipType.HasManyWithMany)
            {
                PropertyInfo property = IsLookupMapping
                    ? LookupModel.EntityModelType.GetProperty(entityModel.Name)
                    : ForeignModel.EntityModelType.GetProperty(ForeignModel.Name);

                string foreignKeyName = null;

                if (!(property is null) &&
                    property.GetCustomAttribute<ForeignKeyAttribute>() is ForeignKeyAttribute foreignKeyAttribute)
                {
                    foreignKeyName = foreignKeyAttribute.Name;
                }
                else
                {
                    foreignKeyName = $"{ForeignModel.Name}ID";
                }

                return foreignKeyNames
                    .Where(x => x.Equals(foreignKeyName, StringComparison.CurrentCultureIgnoreCase))
                    .ToArray();
            }
            else
            {
                return foreignKeyNames;
            }
        }

        public IEnumerable<string> GetForeignKeys<TEntity>()
        {
            return GetForeignKeys(SubSonicContext.DbModel.GetEntityModel<TEntity>());
        }
    }
}
