using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    using Builders;
    using Linq.Expressions;
    using Logging;
    using Schema;

    public class SubSonicTableTypeCollection<TElement>
        : SubSonicTableTypeCollection
        , ISubSonicCollection<TElement>
    {
        public SubSonicTableTypeCollection(string name)
            : base(name, typeof(TElement))
        {

        }

        public SubSonicTableTypeCollection(string name, IQueryProvider provider, Expression expression)
            : base(name, typeof(TElement), provider, expression)
        {

        }

        public SubSonicTableTypeCollection(string name, IQueryProvider provider, Expression expression, IEnumerable<TElement> enumerable)
            : base(name, typeof(TElement), provider, expression, enumerable.Select(x => x as object))
        {

        }

        #region ICollection<> Implementation
        public void Clear()
        {
            if (TableData is ICollection<TElement> data)
            {
                data.Clear();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Add(TElement element)
        {
            if (TableData is ICollection<TElement> data)
            {
                data.Add(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void AddRange(IEnumerable<TElement> elements)
        {
            if (!(elements is null))
            {
                foreach (TElement element in elements)
                {
                    Add(element);
                }
            }
        }

        public bool Remove(TElement element)
        {
            if (TableData is ICollection<TElement> data)
            {
                return data.Remove(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool Contains(TElement element)
        {
            if (TableData is ICollection<TElement> data)
            {
                return data.Contains(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void CopyTo(TElement[] elements, int startAt)
        {
            if (TableData is ICollection<TElement> data)
            {
                data.Select(x => x).ToArray().CopyTo(elements, startAt);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            if (TableData is ICollection<TElement> data)
            {
                return data.GetEnumerator();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        #endregion        
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Generic Class that inherits from this one addresses the generic interface")]
    public class SubSonicTableTypeCollection
        : ISubSonicCollection
    {
        public SubSonicTableTypeCollection(string name, Type elementType)
        {
            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType));

            if (DbContext.DbModel.TryGetEntityModel(elementType, out IDbEntityModel model))
            {
                Model = model;
                Expression = DbExpression.DbSelect(this, GetType(), model.GetTableType(name));
            }

            Provider = new DbSqlTableTypeProvider(name, ElementType, DbContext.ServiceProvider.GetService<ISubSonicLogger>());
        }

        public SubSonicTableTypeCollection(string name, Type elementType, IQueryProvider provider, Expression expression)
            : this(name, elementType)
        {

            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public SubSonicTableTypeCollection(string name, Type elementType, IQueryProvider provider, Expression expression, IEnumerable elements)
            : this(name, elementType, provider, expression)
        {
            TableData = (ICollection)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType), elements);
        }

        public string Name { get; }

        public IDbEntityModel Model { get; }

        public Type ElementType { get; }

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        protected IEnumerable TableData { get; }

        #region ICollection<> Implementation
        public int Count => (int)TableData.GetType().GetProperty(nameof(Count)).GetValue(TableData);
        public bool IsReadOnly => false;
        public IEnumerator GetEnumerator()
        {
            return TableData.GetEnumerator();
        }
        #endregion
    }
}
