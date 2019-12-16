using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public class DbRelationshipMap
        : IDbRelationshipMap
    {
        private readonly string[] foreignKeyNames;

        public DbRelationshipMap(DbRelationshipType relationshipType, IDbEntityModel foreignModel, string[] foreignKeyNames)
        {
            this.foreignKeyNames = foreignKeyNames ?? throw new ArgumentNullException(nameof(foreignKeyNames));
            RelationshipType = relationshipType;
            ForeignModel = foreignModel ?? throw new ArgumentNullException(nameof(foreignModel));
        }

        public DbRelationshipType RelationshipType { get; }

        public IDbEntityModel ForeignModel { get; }

        public IEnumerable<string> GetForeignKeys()
        {
            return foreignKeyNames;
        }
    }
}
