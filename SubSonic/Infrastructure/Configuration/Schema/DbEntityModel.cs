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
            RelationshipMaps = new DbRelationshipMapCollection(this);
            Properties = new DbEntityPropertyCollection(this);
        }

        public ICollection<IDbRelationshipMap> RelationshipMaps { get; }

        public ICollection<IDbEntityProperty> Properties { get; }

        public IDbEntityProperty this[string name] => Properties.SingleOrDefault(property => property.PropertyName == name);

        public IDbEntityProperty this[int index] => Properties.ElementAt(index);

        public bool HasRelationships => RelationshipMaps.Count > 0;

        public Type EntityModelType { get; internal set; }

        public DbTableExpression Expression => new DbTableExpression(this);

        public IDbRelationshipMap GetRelationshipWith(IDbEntityModel model)
        {
            if (model.IsNotNull())
            {
                foreach (IDbRelationshipMap map in RelationshipMaps)
                {
                    if (map.ForeignModel.QualifiedName == model.QualifiedName)
                    {
                        return map;
                    }
                }
            }
            return null;
        }

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
