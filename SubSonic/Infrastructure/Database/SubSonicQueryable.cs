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

    public sealed class SubSonicCollection<TElement>
        : SubSonicCollection
        , ISubSonicDbCollection<TElement>
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
            : base(typeof(TElement), provider, expression, enumerable.Select(x => x as object))
        {

        }

        #region ICollection<> Implementation
        public void Clear() => TableData.Clear();
        public void Add(TElement element)
        {
            TableData.Add(element);
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
            return TableData.Remove(element);
        }

        public bool Contains(TElement element)
        {
            return TableData.Contains(element);
        }

        public void CopyTo(TElement[]  elements, int startAt)
        {
            TableData.Select(x => (TElement)x).ToArray().CopyTo(elements, startAt);
        }
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return TableData.Convert<TElement>().GetEnumerator();
        }
        #endregion

        public IQueryable<TElement> FindByID(params object[] keyData)
        {
            return FindByID(keyData, DbContext.DbModel.GetEntityModel<TElement>().GetPrimaryKey().ToArray());
        }

        public IQueryable<TElement> FindByID(object[] keyData, params string[] keyNames)
        {
            if (keyData is null)
            {
                throw new ArgumentNullException(nameof(keyData));
            }

            if (keyNames is null)
            {
                throw new ArgumentNullException(nameof(keyNames));
            }

            if (Expression is DbSelectExpression select)
            {
                ISubSonicQueryProvider<TElement> builder = (ISubSonicQueryProvider<TElement>)Provider;

                Expression
                    logical = null;

                for (int i = 0; i < keyNames.Length; i++)
                {
                    logical = builder.BuildLogicalBinary(logical, keyNames[i], keyData[i], DbComparisonOperator.Equal, DbGroupOperator.AndAlso);
                }

                LambdaExpression predicate = (LambdaExpression)builder.BuildLambda(logical, LambdaType.Predicate);

                Expression where = builder.BuildWhere(select.From, null, typeof(TElement), predicate);

                return builder.CreateQuery<TElement>(builder.BuildSelect(select, where));
            }

            throw new NotSupportedException();
        }

        public IQueryable<TElement> Load()
        {
            Linq.SubSonicQueryable.Load(this);

            return this;
        }
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
                Model = model;

                Expression = DbExpression.DbSelect(this, GetType(), Model.Table);
            }
            else
            {
                Expression = Expression.Constant(this);
            }
        }
        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression)
            : this(elementType)
        {
            TableData = new HashSet<object>();
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Provider = provider ?? new DbSqlQueryBuilder(ElementType, DbContext.ServiceProvider.GetService<ISubSonicLogger>());
        }

        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression, IEnumerable<object> elements)
            : this(elementType, provider, expression)
        {
            TableData = new HashSet<object>(elements);

            if (TableData.Count > 0 && !TableData.ElementAt(0).IsOfType(elementType))
            {
                throw new NotSupportedException();
            }
        }

        protected IDbEntityModel Model { get; }

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
