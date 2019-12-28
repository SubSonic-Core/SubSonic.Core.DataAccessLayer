using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace SubSonic.Infrastructure
{
    using Linq;
    using Linq.Expressions;
    using Linq.Expressions.Alias;
    using Schema;
    using Data.DynamicProxies;

    public class DbSetCollection<TEntity>
        : ISubSonicCollection<TEntity>
    {
        private readonly IQueryProvider provider;
        private readonly IDbEntityModel model;
        private readonly ICollection<TEntity> queryableData;
        
        public DbSetCollection(ISubSonicQueryProvider<TEntity> provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            queryableData = new ObservableCollection<TEntity>();
            model = DbContext.Model.GetEntityModel<TEntity>();
            Expression = DbExpression.DbSelect(this, model.Table);
        }

        protected DbContext DbContext => DbContext.ServiceProvider.GetService<DbContext>();

        public DbSetCollection(ISubSonicQueryProvider<TEntity> provider, Expression expression)
            : this(provider)
        {
            this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider => provider;

        #region ICollection<TEntity> Implementation
        public void Add(TEntity entity)
        {
            if (!(entity is IEntityProxy))
            {
                entity = DynamicProxy.MapInstanceOf(DbContext, entity);
            }

            queryableData.Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (TEntity entity in entities)
            {
                queryableData.Add(entity);
            }
        }

        public bool Remove(TEntity entity)
        {
            return queryableData.Remove(entity);
        }

        public bool Contains(TEntity entity) => queryableData.Contains(entity);

        public void CopyTo(TEntity[] entities, int startAt) => queryableData.CopyTo(entities, startAt);

        public void Clear() => queryableData.Clear();

        public int Count => queryableData.Count;

        public bool IsReadOnly => false;

        public IEnumerator GetEnumerator()
        {
            if (queryableData.Count == 0)
            {
                Load();
            }
            return ((IEnumerable)queryableData).GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            if (queryableData.Count == 0)
            {
                Load();
            }
            return queryableData.GetEnumerator();
        }

        private IQueryable<TEntity> Load()
        {
            AddRange(SubSonicQueryable.Load(this.Select()));

            return this;
        }
        #endregion

        public IQueryable<TEntity> FindByID(params object[] keyData)
        {
            return FindByID(keyData, model.GetPrimaryKey().ToArray());
        }

        public IQueryable<TEntity> FindByID(object[] keyData, params string[] keyNames)
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
                ISubSonicQueryProvider<TEntity> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<TEntity>>();

                Expression
                    logical = null;

                for (int i = 0; i < keyNames.Length; i++)
                {
                    logical = builder.BuildLogicalBinary(logical, DbExpressionType.Where, keyNames[i], keyData[i], DbComparisonOperator.Equal, DbGroupOperator.AndAlso);
                }

                LambdaExpression predicate = (LambdaExpression)builder.BuildLambda(logical, LambdaType.Predicate);

                Expression where = builder.BuildWhere(select.From, null, typeof(TEntity), predicate);

                return builder.CreateQuery<TEntity>(builder.BuildSelect(select, where));
            }

            throw new NotSupportedException();
        }
    }
}

