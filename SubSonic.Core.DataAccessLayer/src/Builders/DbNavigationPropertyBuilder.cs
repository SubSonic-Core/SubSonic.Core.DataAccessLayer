using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic
{
    using Linq;
    using Schema;
    using SubSonic.src;
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

            this.RelatedEntityType = typeof(TRelatedEntity).GetQualifiedType();
        }
        public DbNavigationPropertyBuilder(string has, string with)
            : this(has)
        {
            this.with = with ?? throw new ArgumentNullException(nameof(with));
        }

        public DbRelationshipType RelationshipType => (DbRelationshipType)Enum.Parse(typeof(DbRelationshipType), $"{has}{with}");

        public bool IsReciprocated { get; private set; }

        public Type RelatedEntityType { get; private set; }

        public Type LookupEntityType { get; private set; }

        public IEnumerable<string> RelatedKeys { get; private set; }

        public string PropertyName { get; set; }

        public IDbRelationshipMap RelationshipMap => new DbRelationshipMap(
                        RelationshipType,
                        SubSonicContext.DbModel.GetEntityModel(LookupEntityType),
                        SubSonicContext.DbModel.GetEntityModel(RelatedEntityType),
                        PropertyName,
                        RelatedKeys.ToArray());

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> selector)
        {
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));
            }
            if (RelatedEntityType is null)
            {
                throw Error.InvalidOperation();
            }

            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithOne))
            {
                RelatedKeys = GetForeignKeys(selector.Body),
                PropertyName = this.PropertyName
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithNone()
        {
            if (RelatedEntityType is null)
            {
                throw Error.InvalidOperation();
            }

            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithNone))
            {
                RelatedKeys = GetForeignKeys(typeof(TEntity)),
                PropertyName = this.PropertyName
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithMany(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> selector = null)
        {
            if (RelatedEntityType is null)
            {
                throw Error.InvalidOperation();
            }

            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithMany))
            {
                IsReciprocated = !(selector is null),
                LookupEntityType = LookupEntityType,
                RelatedKeys = (RelatedKeys?.Any() ?? false) ? RelatedKeys : GetPrimayKeys(selector, LookupEntityType),
                PropertyName = this.PropertyName
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> UsingLookup<TLookupEntity>(Expression<Func<TRelatedEntity, TLookupEntity>> selector)
            where TLookupEntity: class
        {
            if (selector is null)
            {
                throw Error.ArgumentNull(nameof(selector));
            }

            if (RelatedEntityType is null)
            {
                throw Error.InvalidOperation();
            }

            Type
                lookupEntityType = typeof(TLookupEntity).GetQualifiedType();

            LookupEntityType = lookupEntityType;
            RelatedKeys = GetPrimayKeys(selector, lookupEntityType);

            return this;
        }

        private string[] GetForeignKeys(Type type)
        {
            return Ext.GetForeignKeyName(type);
        }

        private string[] GetForeignKeys(Expression expression)
        {
            if(expression is MemberExpression TheMember)
            {
                if (TheMember.Member is PropertyInfo property)
                {
                    return Ext.GetForeignKeyName(property);
                }
            }
            return Array.Empty<string>();
        }

        private string[] GetPrimayKeys(Expression expression, Type lookupEntityType)
        {
            if (expression.IsNotNull())
            {
                if (lookupEntityType is null)
                {
                    return Ext.GetPrimaryKeyName<TRelatedEntity>();
                }
                else
                {
                    return GetForeignKeys(lookupEntityType);
                }
            }
            return Array.Empty<string>();
        }

    }
}
