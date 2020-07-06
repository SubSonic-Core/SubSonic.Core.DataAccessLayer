using System;
using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Infrastructure.Schema
{
    using Linq;

    public class DbRelationshipMap
        : IDbRelationshipMap
    {
        private readonly string[] foreignKeyNames;

        public DbRelationshipMap(
            DbRelationshipType relationshipType,
            IDbEntityModel lookupModel,
            IDbEntityModel foreignModel, 
            string[] foreignKeyNames)
        {
            if((relationshipType == DbRelationshipType.HasManyWithMany) && (lookupModel is null))
            {
                throw new ArgumentNullException(nameof(lookupModel));
            }

            this.foreignKeyNames = foreignKeyNames ?? throw new ArgumentNullException(nameof(foreignKeyNames));
            RelationshipType = relationshipType;
            LookupModel = lookupModel;
            ForeignModel = foreignModel ?? throw new ArgumentNullException(nameof(foreignModel));
            
        }

        public DbRelationshipType RelationshipType { get; }

        public IDbEntityModel LookupModel { get; }

        public IDbEntityModel ForeignModel { get; }

        public IEnumerable<string> GetForeignKeys()
        {
            if (RelationshipType == DbRelationshipType.HasManyWithMany)
            {
                return foreignKeyNames
                    .Where(x => !x.StartsWith(ForeignModel.Name, StringComparison.CurrentCultureIgnoreCase))
                    .ToArray();
            }
            else
            {
                return foreignKeyNames;
            }
        }
    }
}
