using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public enum DbEntityPropertyType
    {
        Unknown = 0,
        Value,
        Navigation,
        Collection,
        DAL
    }
}
