using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public class DbObject
        : IDbObject
    {
        protected DbObject()
        {
            SchemaName = "dbo";
        }

        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string QualifiedName { get => $"{SchemaName}.{Name}".EncapsulateQualifiedName(); }
        public string SchemaName { get; set; }

        public override string ToString()
        {
            return QualifiedName;
        }
    }
}
