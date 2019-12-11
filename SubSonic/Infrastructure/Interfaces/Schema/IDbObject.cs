using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public interface IDbObject
    {
        string Name { get; set; }
        string FriendlyName { get; set; }
        string QualifiedName { get; }
        string SchemaName { get; set; }
    }
}
