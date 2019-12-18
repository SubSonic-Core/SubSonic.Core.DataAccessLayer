using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace SubSonic.Infrastructure
{
    using Schema;
    using Builders;
    using Logging;
    
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
            : base(typeof(TElement), provider, expression, Array.ConvertAll(enumerable.ToArray(), (ele) => ele as object))
        {

        }

        #region ICollection<> Implementation
        public void Clear() => TableData.Clear();
        public void Add(TElement element)
        {
            TableData.Add(element);
        }

        public bool Remove(TElement element)
        {
            return TableData.Remove(element);
        }

        public bool Contains(TElement element)
        {
            return TableData.Contains(element);
        }

        public void CopyTo(TElement[]  elements, int startAt)
        {
            TableData.CopyTo(Array.ConvertAll(elements, x => x as object), startAt);
        }
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return TableData.Convert<TElement>().GetEnumerator();
        }
        #endregion
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Generic Class that inherits from this one addresses the generic interface")]
    public class SubSonicCollection
        : ISubSonicCollection
    {
        public SubSonicCollection(Type elementType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));

            if (DbContext.DbModel.TryGetEntityModel(elementType, out IDbEntityModel model))
            {
                Expression = model.Expression;
            }
            else
            {
                Expression = Expression.Parameter(elementType);
            }
        }
        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression)
            : this(elementType)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Provider = provider ?? new DbSqlQueryBuilder(ElementType, DbContext.ServiceProvider.GetService<ISubSonicLogger>());
            TableData = new ObservableCollection<object>();
        }

        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression, IEnumerable<object> elements)
            : this(elementType, provider, expression)
        {
            TableData = new List<object>(elements);

            if (!TableData.ElementAt(0).IsOfType(elementType))
            {
                throw new NotSupportedException();
            }
        }

        protected ICollection<object> TableData { get; }

        public Type ElementType { get; }

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        #region ICollection<> Implementation
        public int Count => TableData.Count;
        public bool IsReadOnly => TableData.IsReadOnly;
        public IEnumerator GetEnumerator()
        {
            return TableData.GetEnumerator();
        }
        #endregion

    }
}
