using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Linq.Expressions;
    using Linq.Expressions.Alias;
    using Schema;

    public class DbEntityModel
        : DbObject
        , IDbEntityModel
    {
        private IEnumerable<string> primaryKey;

        public DbEntityModel()
        {
            RelationalMappings = new DbRelationalMappingCollection(this);
            Properties = new DbEntityPropertyCollection(this);
        }

        public ICollection<IDbRelationalMapping> RelationalMappings { get; }

        public ICollection<IDbEntityProperty> Properties { get; }

        public IDbEntityProperty this[string name] => Properties.SingleOrDefault(property => property.PropertyName == name);

        public IDbEntityProperty this[int index] => Properties.ElementAt(index);

        public bool HasRelationships { get; }

        public Type EntityModelType { get; internal set; }

        public DbTableExpression Expression => new DbTableExpression(EntityModelType, new TableAlias(Name), QualifiedName);

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
