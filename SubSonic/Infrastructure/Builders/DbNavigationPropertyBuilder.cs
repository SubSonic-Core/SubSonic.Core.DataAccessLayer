using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SubSonic.Infrastructure
{
    using Linq;
    using Schema;
    using Ext = SubSonic.SubSonicExtensions;

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

        public IEnumerable<string> RelatedKeys { get; private set; }

        public IDbRelationshipMap RelationshipMap => new DbRelationshipMap(RelationshipType, DbContext.DbModel.GetEntityModel<TRelatedEntity>(), RelatedKeys.ToArray());

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> selector = null)
        {
            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithOne))
            {
                RelatedKeys = selector.IsNull() ? Array.Empty<string>() : GetForeignKeys(selector.Body)
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> WithMany(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> selector = null)
        {
            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(has, nameof(WithMany))
            {
                RelatedKeys = selector.IsNull() ? Array.Empty<string>() : GetPrimayKeys(selector.Body)
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
