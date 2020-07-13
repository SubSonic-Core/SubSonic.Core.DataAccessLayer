using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Schema
{
    public interface IDbRelationshipMap
    {
        DbRelationshipType RelationshipType { get; }

        IDbEntityModel ForeignModel { get; }
        IDbEntityModel LookupModel { get; }

        IEnumerable<string> GetForeignKeys();
    }
}
