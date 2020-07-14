using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Schema;

    public interface IDbRelationalMapping
    {
        IDbEntityModel EntityModel { get; }
    }
}
