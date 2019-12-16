using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IDbNavigationPropertyBuilder
    {
        Infrastructure.Schema.IDbRelationshipMap RelationshipMap { get; }
    }
}
