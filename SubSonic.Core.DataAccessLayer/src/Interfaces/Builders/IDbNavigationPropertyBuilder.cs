using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IDbNavigationPropertyBuilder
    {
        Schema.IDbRelationshipMap RelationshipMap { get; }
    }
}
