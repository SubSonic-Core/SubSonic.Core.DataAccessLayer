using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public interface IDbRelationshipMap
    {
        DbRelationshipType RelationshipType { get; }

        IDbEntityModel ForeignModel { get; }

        IEnumerable<string> GetForeignKeys();
    }
}
