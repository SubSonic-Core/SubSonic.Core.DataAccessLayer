using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Logging;
    using Builders;

    public sealed class SubSonicCollection<TElement>
        : SubSonicCollection
        , ISubSonicCollection<TElement>
    {
        public SubSonicCollection()
            : base(typeof(TElement))
        {

        }
        public SubSonicCollection(IQueryProvider provider, Expression expression)
            : base(typeof(TElement), provider, expression)
        {

        }

        public SubSonicCollection(IQueryProvider provider, Expression expression, IEnumerable<TElement> enumerable)
            : base(typeof(TElement), provider, expression, enumerable)
        {

        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return Table.Convert<TElement>().GetEnumerator();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Generic Class that inherits from this one addresses the generic interface")]
    public class SubSonicCollection
        : ISubSonicCollection
    {
        public SubSonicCollection(Type elementType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            Expression = DbContext.DbModel.GetEntityModel(elementType).Expression;
        }
        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression)
            : this(elementType)
        {
            Expression = expression ?? DbContext.DbModel.GetEntityModel(elementType).Expression;
            Provider = provider ?? new DbSqlQueryBuilder(ElementType, DbContext.ServiceProvider.GetService<ISubSonicLogger>());
            Table = new ArrayList();
        }

        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression, IEnumerable enumerable)
            : this(elementType, provider, expression)
        {
            Table = new ArrayList(enumerable as ICollection);

            if (!Table[0].IsOfType(elementType))
            {
                throw new NotSupportedException();
            }
        }

        protected ArrayList Table { get; }

        public Type ElementType { get; }

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IEnumerator GetEnumerator()
        {
            return Table.GetEnumerator();
        }
    }
}
