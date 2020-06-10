using System;
using System.Collections.Generic;

namespace SubSonic.Infrastructure.Schema
{
    using Linq;
    using Linq.Expressions;
    using Schema;

    public class DbEntityModel
        : DbObject
        , IDbEntityModel
    {
        private IEnumerable<string> primaryKey;

        public DbEntityModel()
            : this("dbo") { }

        public DbEntityModel(string schemaName)
            : base(schemaName)
        {
            Commands = new DbCommandQueryCollection()
            {
                new DbCommandQuery(DbQueryType.Insert),
                new DbCommandQuery(DbQueryType.Update),
                new DbCommandQuery(DbQueryType.Delete),
            };

            RelationshipMaps = new DbRelationshipMapCollection(this);
            Properties = new DbEntityPropertyCollection(this);
        }

        public DbCommandQueryCollection Commands { get; }

        public ICollection<IDbRelationshipMap> RelationshipMaps { get; }

        public ICollection<IDbEntityProperty> Properties { get; }

        public IDbEntityProperty this[string name] => Properties.SingleOrDefault(property => property.PropertyName == name);

        public IDbEntityProperty this[int index] => Properties.ElementAt(index);

        public bool HasRelationships => RelationshipMaps.Count > 0;

        public Type EntityModelType { get; internal set; }

        public DbTableExpression Table => (DbTableExpression)DbExpression.DbTable(this);

        public bool DefinedTableTypeExists => DefinedTableType.IsNotNull();

        public IDbObject DefinedTableType { get; set; }

        public IDbRelationshipMap GetRelationshipWith(IDbEntityModel model)
        {
            if (model.IsNotNull())
            {
                foreach (IDbRelationshipMap map in RelationshipMaps)
                {
                    if (map.ForeignModel.QualifiedName == model.QualifiedName ||
                        map.LookupModel?.QualifiedName == model.QualifiedName)
                    {
                        return map;
                    }
                }
            }
            return null;
        }

        public object CreateObject()
        {
            return DbContext.CreateObject(EntityModelType);
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
