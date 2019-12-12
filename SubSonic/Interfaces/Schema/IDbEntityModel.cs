using System;
using System.Collections.Generic;

namespace SubSonic.Infrastructure.Schema
{
    using Linq.Expressions;
    public interface IDbEntityModel
        : IDbObject
    {
        IDbEntityProperty this[string name] { get; }
        IDbEntityProperty this[int index] { get; }

        ICollection<IDbRelationalMapping> RelationalMappings { get; }
        ICollection<IDbEntityProperty> Properties { get; }
        bool HasRelationships { get; }
        Type EntityModelType { get; }
        DbTableExpression Expression { get; }

        IEnumerable<string> GetPrimaryKey();
    }
}
