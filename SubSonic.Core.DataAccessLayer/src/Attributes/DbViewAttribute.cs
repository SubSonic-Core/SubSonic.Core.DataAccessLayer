using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DbViewAttribute
        : Attribute
    {
        public DbViewAttribute()
        {
        }

        public DbViewAttribute(string name, string schema = "dbo")
        {
            Name = name;
            Schema = schema;
        }

        public string Query { get; set; }
        public string Name { get; set; }
        public string Schema { get; private set; }
    }
}
