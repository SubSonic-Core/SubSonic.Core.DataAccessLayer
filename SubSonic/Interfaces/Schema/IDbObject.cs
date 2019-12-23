using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public interface IDbObject
    {
        string Name { get; }
        string FriendlyName { get; }
        string QualifiedName { get; }
        string SchemaName { get; }
    }
}
