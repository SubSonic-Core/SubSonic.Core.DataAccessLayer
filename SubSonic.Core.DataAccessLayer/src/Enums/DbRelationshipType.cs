using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public enum DbRelationshipType
    {
        Unknown = 0,
        HasOneWithNone,
        HasOneWithOne,
        HasManyWithOne,
        HasOneWithMany,
        HasManyWithMany
    }
}
