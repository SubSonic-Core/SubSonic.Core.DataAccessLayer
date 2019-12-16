using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public enum DbRelationshipType
    {
        Unknown = 0,
        HasOneWithOne,
        HasManyWithOne,
        HasOneWithMany,
        HasManyWithMany
    }
}
