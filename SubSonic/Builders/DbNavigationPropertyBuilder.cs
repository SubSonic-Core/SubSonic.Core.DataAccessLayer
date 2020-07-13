using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic
{
    using Linq;
    using Schema;
    using Ext = SubSonicExtensions;

    public class DbNavigationPropertyBuilder<TEntity, TRelatedEntity>
        : IDbNavigationPropertyBuilder
        where TEntity : class
        where TRelatedEntity : class
    {
        private readonly string has, with;
        public DbNavigationPropertyBuilder(string has)
        {
            this.has = has ?? throw new ArgumentNullException(nameof(has));
        }
        public DbNavigationPropertyBuilder(string has, string with)
            : this(has)
        {
            this.with = with ?? throw new ArgumentNullException(nameof(with));
        }

        public DbRelationshipType RelationshipType => (DbRelationshipType)Enum.Parse(typeof(DbRelationshipType), $"{has}{with}");

        public Type RelatedEntityType { get; private set; }

        public Type LookupEntityType { get; private set; }

        public IEnumerable<string> RelatedKeys { get; private set; }

        public IDbRelationshipMap RelationshipMap => new DbRelationshipMap(RelationshipType, SubSonicContext.DbModel.GetEntityModel(LookupEntityType), SubSonicContext.DbModel.GetEntityModel(RelatedEntityType), RelatedKeys.ToArray());

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> selector = null)
        {
            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithOne))
            {
                RelatedEntityType = typeof(TRelatedEntity).GetQualifiedType(),
                RelatedKeys = (selector is null) ? Array.Empty<string>() : GetForeignKeys(selector.Body)
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithMany(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> selector = null)
        {
            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithMany))
            {
                RelatedEntityType = typeof(TRelatedEntity).GetQualifiedType(),
                RelatedKeys = (selector is null) ? Array.Empty<string>() : GetPrimayKeys(selector.Body)
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TLookupEntity> WithMany<TLookupEntity>(Expression<Func<TRelatedEntity, TLookupEntity>> selector = null)
            where TLookupEntity : class
        {
            return new DbNavigationPropertyBuilder<TEntity, TLookupEntity>(has, nameof(WithMany))
            {
                LookupEntityType = typeof(TRelatedEntity).GetQualifiedType(),
                RelatedEntityType = typeof(TLookupEntity).GetQualifiedType(),
                RelatedKeys = (selector is null) ? Array.Empty<string>() : GetPrimayKeys(selector.Body)
            };
        }

        private string[] GetForeignKeys(Expression expression)
        {
            if(expression.IsNotNull())
            {
                return Ext.GetForeignKeyName((PropertyInfo)((MemberExpression)expression).Member);
            }
            return Array.Empty<string>();
        }

        private string[] GetPrimayKeys(Expression expression)
        {
            if (expression.IsNotNull())
            {
                return Ext.GetPrimaryKeyName<TRelatedEntity>();
            }
            return Array.Empty<string>();
        }

    }
}
