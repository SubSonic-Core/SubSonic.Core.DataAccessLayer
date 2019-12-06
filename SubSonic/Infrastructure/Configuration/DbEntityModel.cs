using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbEntityModel
    {
        public DbEntityModel()
        {
            Properties = new List<DbEntityProperty>();
        }

        public ICollection<DbEntityProperty> Properties { get; }

        public Type EntityModelType { get; internal set; }
        public string[] PrimaryKey { get; internal set; }
    }
}
