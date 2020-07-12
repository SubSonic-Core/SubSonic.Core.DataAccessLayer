using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Collections
{
    using Interfaces;
    using Infrastructure.Builders;
    using Linq.Expressions;
    using Infrastructure.Logging;
    using Infrastructure.Schema;

    public sealed partial class SubSonicTableTypeCollection<TEntity>
        : SubSonicTableTypeCollection
        , ISubSonicCollection<TEntity>
    {
        public SubSonicTableTypeCollection(string name)
            : base(name, typeof(TEntity))
        {

        }

        public SubSonicTableTypeCollection(string name, IQueryProvider provider, Expression expression)
            : base(name, typeof(TEntity), provider, expression)
        {

        }

        public SubSonicTableTypeCollection(string name, IQueryProvider provider, Expression expression, IEnumerable<TEntity> enumerable)
            : base(name, typeof(TEntity), provider, expression, enumerable)
        {

        }

        #region ICollection<> Implementation
        public void Clear()
        {
            if (TableData is ICollection<TEntity> data)
            {
                data.Clear();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Add(TEntity element)
        {
            if (TableData is ICollection<TEntity> data)
            {
                if (!IsReadOnly)
                {
                    data.Add(element);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void AddRange(IEnumerable<TEntity> elements)
        {
            if (!(elements is null))
            {
                foreach (TEntity element in elements)
                {
                    Add(element);
                }
            }
        }

        public bool Remove(TEntity element)
        {
            if (TableData is ICollection<TEntity> data)
            {
                return data.Remove(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool Contains(TEntity element)
        {
            if (TableData is ICollection<TEntity> data)
            {
                return data.Contains(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void CopyTo(TEntity[] elements, int startAt)
        {
            if (TableData is ICollection<TEntity> data)
            {
                data.Select(x => x).ToArray().CopyTo(elements, startAt);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            if (TableData is ICollection<TEntity> data)
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
        /// <summary>
        /// get a new instance of a table type collection <see cref="SubSonicTableTypeCollection"/>
        /// </summary>
        /// <param name="name">the name of the table type</param>
        /// <param name="elementType">clr type of the table type</param>
        public SubSonicTableTypeCollection(string name, Type elementType)
        {
            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType));

            if (SubSonicContext.DbModel.TryGetEntityModel(elementType, out IDbEntityModel model))
            {
                Model = model;
                Expression = DbExpression.DbSelect(this, GetType(), model.GetTableType(name));
                Provider = new DbSqlTableTypeProvider(name, ElementType, SubSonicContext.ServiceProvider.GetService<ISubSonicLogger>());
            }
        }

        /// <summary>
        /// get a new instance of a table type collection <see cref="SubSonicTableTypeCollection"/>
        /// </summary>
        /// <param name="name">the name of the table type</param>
        /// <param name="elementType">clr type of the table type</param>
        /// <param name="provider">the query provider <see cref="IQueryProvider"/></param>
        /// <param name="expression">the <see cref="DbExpression"> representing the query</param>
        public SubSonicTableTypeCollection(string name, Type elementType, IQueryProvider provider, Expression expression)
            : this(name, elementType)
        {
            Expression = expression ?? DbExpression.DbSelect(this, GetType(), Model.GetTableType(name));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// get a new instance of a table type collection <see cref="SubSonicTableTypeCollection"/>
        /// </summary>
        /// <param name="name">the name of the table type</param>
        /// <param name="elementType">clr type of the table type</param>
        /// <param name="provider">the query provider <see cref="IQueryProvider"/></param>
        /// <param name="expression">the <see cref="DbExpression"> representing the query</param>
        /// <param name="elements"><see cref="IEnumerable" /> of elements of <see cref="ElementType"/></param>
        public SubSonicTableTypeCollection(string name, Type elementType, IQueryProvider provider, Expression expression,
                                           IEnumerable elements)
            : this(name, elementType, provider, expression)
        {
            TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType), elements);
        }

        public string Name { get; }

        public IDbEntityModel Model { get; }

        public Type ElementType { get; }

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        protected IEnumerable TableData { get; private set; }

        public IQueryable Load()
        {
            if (Provider.Execute(Expression) is IEnumerable elements)
            {
                TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(ElementType), elements);
            }

            return this;
        }

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
