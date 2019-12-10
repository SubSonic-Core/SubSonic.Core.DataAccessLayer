using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;

    public interface IDbRelationalMapping
    {
        IDbEntityModel EntityModel { get; }
    }
}
