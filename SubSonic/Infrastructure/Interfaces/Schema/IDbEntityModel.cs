using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public interface IDbEntityModel
        : IDbObject
    {
        IDbEntityProperty this[string name] { get; }
        IDbEntityProperty this[int index] { get; }

        ICollection<IDbRelationalMapping> RelationalMappings { get; }
        ICollection<IDbEntityProperty> Properties { get; }
        bool HasRelationships { get; }
        Type EntityModelType { get; }

        IEnumerable<string> GetPrimaryKey();
    }
}
