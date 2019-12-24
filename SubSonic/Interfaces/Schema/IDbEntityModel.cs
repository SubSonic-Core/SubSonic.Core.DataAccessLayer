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

        DbCommandQueryCollection Commands { get; }
        ICollection<IDbRelationshipMap> RelationshipMaps { get; }
        ICollection<IDbEntityProperty> Properties { get; }
        bool HasRelationships { get; }
        Type EntityModelType { get; }
        DbTableExpression Table { get; }

        object CreateObject();
        IEnumerable<string> GetPrimaryKey();
        IDbRelationshipMap GetRelationshipWith(IDbEntityModel model);
    }
}
