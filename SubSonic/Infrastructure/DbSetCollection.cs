using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    using Linq;
    using Linq.Expressions;
    using Linq.Expressions.Alias;

    public class DbSetCollection<TEntity>
        : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable, IListSource
    {
        private readonly IQueryProvider provider;
        private readonly DbEntityModel model;
        private readonly List<TEntity> queryableData;
        
        public DbSetCollection(ISubSonicQueryProvider<TEntity> provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            queryableData = new List<TEntity>();
            model = DbContext.Model.GetEntityModel<TEntity>();
            Expression = model.Expression;
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

        public bool ContainsListCollection => true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerator GetEnumerator()
        {
            return GetList().GetEnumerator();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IList GetList()
        {
            return queryableData;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return ((IEnumerable<TEntity>)queryableData).GetEnumerator();
        }

        public IQueryable<TEntity> FindByID(params object[] keyData)
        {
            ISubSonicQueryProvider<TEntity> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<TEntity>>();

            Expression 
                body = null, 
                where = null;

            string[] keys = model.GetPrimaryKey().ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
               body = builder.BuildComparisonExpression(body, keys[i], keyData[i], ComparisonOperator.Equal, GroupOperator.AndAlso);
            }

            where = builder.CallExpression(queryableData.AsQueryable().Expression, body, ExpressionCallType.Where);

            return builder.CreateQuery<TEntity>(new DbSelectExpression(model.ToAlias(), model.Properties.ToColumnList((DbTableExpression)Expression), Expression, where));

            //

            //for (int i = 0; i < keys.Length; i++)
            //{
            //    builder.BuildComparisonExpression(keys[i], keyData[i], ComparisonOperator.Equal, GroupOperator.AndAlso);
            //}

            //return Provider.CreateQuery<TEntity>(
            //    builder
            //        .CallExpression<TEntity>(ExpressionCallType.Where)
            //        .ForEachProperty(keys, property => 
            //            builder.CallExpression<TEntity>(ExpressionCallType.OrderBy, property))
            //        .ToMethodCallExpression());
        }
    }
}
