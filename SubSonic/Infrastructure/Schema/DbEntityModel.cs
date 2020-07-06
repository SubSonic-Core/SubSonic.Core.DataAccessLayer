using System;
using System.Linq;
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

        public DbCommandQueryCollection Commands { get; internal set; }

        public ICollection<IDbRelationshipMap> RelationshipMaps { get; internal set; }

        public ICollection<IDbEntityProperty> Properties { get; internal set; }

        public IDbEntityProperty this[string name] => Properties.SingleOrDefault(property => property.PropertyName == name);

        public IDbEntityProperty this[int index] => Properties.ElementAt(index);

        public bool HasRelationships => RelationshipMaps.Count > 0;

        public Type EntityModelType { get; internal set; }

        public DbTableExpression Table => (DbTableExpression)DbExpression.DbTable(this, this.ToAlias());

        public DbTableExpression GetTableType(string name)
        {
            if ((name ?? Name).IsNullOrEmpty())
            {
                throw new InvalidOperationException();
            }

            if (DbExpression.DbTableType(this, name ?? Name) is DbTableExpression expression)
            {
                return expression;
            }

            return null;
        }

        public bool DefinedTableTypeExists => DefinedTableType.IsNotNull();

        public IDbObject DefinedTableType { get; internal set; }

        public int ObjectGraphWeight { get; private set; }

        public void IncrementObjectGraphWeight()
        {
            if (!DbContext.Current.IsDbModelReadOnly)
            {
                ObjectGraphWeight++;
            }
        }

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
            return Activator.CreateInstance(EntityModelType);
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
