using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Schema;

    public class DbRelationshipBuilder<TEntity>
        where TEntity : class
    {
        private readonly IDbEntityModel primary;

        public DbRelationshipBuilder(IDbEntityModel primary)
        {
            this.primary = primary ?? throw new ArgumentNullException(nameof(primary));
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> selector) where TRelatedEntity : class
        {
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            SubSonicContext.DbModel.GetEntityModel<TRelatedEntity>().IncrementObjectGraphWeight();

            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(nameof(HasMany))
            {
                PropertyName = selector.GetPropertyName()
            };
        }

        public DbNavigationPropertyBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> selector) where TRelatedEntity : class
        {
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            primary.IncrementObjectGraphWeight();

            return new DbNavigationPropertyBuilder<TEntity, TRelatedEntity>(nameof(HasOne))
            {
                PropertyName = selector.GetPropertyName()
            };
        }
    }
}
