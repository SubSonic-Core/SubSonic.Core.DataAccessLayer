using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Schema
{
    public class DbObject
        : IDbObject
    {
        protected DbObject()
        {
            SchemaName = "dbo";
        }

        protected DbObject(string schemaName)
            : this()
        {
            if (schemaName.IsNotNullOrEmpty())
            {
                SchemaName = schemaName;
            }
        }

        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string QualifiedName { get => $"{SchemaName}.{Name}".EncapsulateQualifiedName(); }
        public string SchemaName { get; set; }
        public DbObjectTypeEnum DbObjectType { get; set; }

        public override string ToString() => QualifiedName;
    }
}
