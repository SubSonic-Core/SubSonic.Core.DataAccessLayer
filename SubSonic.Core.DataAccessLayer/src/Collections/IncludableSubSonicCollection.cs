using SubSonic.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.src.Collections
{
    internal class IncludableSubSonicCollection<TEntity, TProperty>
        : SubSonicCollection<TEntity>
        , IIncludableQueryable<TEntity, TProperty>
    {
        public IncludableSubSonicCollection(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {

        }

        public IncludableSubSonicCollection(IQueryProvider provider, Expression expression, IEnumerable<TEntity> enumerable)
            : base(provider, expression, enumerable)
        {

        }

        public Type PropertyType => typeof(TProperty);
    }
}
