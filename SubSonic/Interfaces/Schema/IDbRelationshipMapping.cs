using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public interface IDbRelationshipMapping
    {
        IEnumerable<string> GetForeignKeys();
    }
}
