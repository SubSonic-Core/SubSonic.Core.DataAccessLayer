using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbEntityModel
    {
        private IEnumerable<string> primaryKey;

        public DbEntityModel()
        {
            Properties = new List<DbEntityProperty>();
        }

        public ICollection<DbEntityProperty> Properties { get; }

        public DbEntityProperty this[string name] => Properties.SingleOrDefault(property => property.RuntimeName == name);

        public DbEntityProperty this[int index] => Properties.ElementAt(index);

        public Type EntityModelType { get; internal set; }

        public IEnumerable<string> GetPrimaryKey()
        {
            return primaryKey;
        }

        internal void SetPrimaryKey(IEnumerable<string> value)
        {
            primaryKey = value;
        }
    }
}
