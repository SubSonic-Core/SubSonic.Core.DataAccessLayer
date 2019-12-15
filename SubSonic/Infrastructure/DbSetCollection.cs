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
        : ISubSonicCollection<TEntity>, IListSource
    {
        private readonly IQueryProvider provider;
        private readonly DbEntityModel model;
        private readonly List<TEntity> queryableData;
        
        public DbSetCollection(ISubSonicQueryProvider<TEntity> provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            queryableData = new List<TEntity>();
            model = DbContext.Model.GetEntityModel<TEntity>();
            Expression = provider.GetAliasedTable();
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

        public ISubSonicCollection<TEntity> FindByID(params object[] keyData)
        {
            ISubSonicQueryProvider<TEntity> builder = DbContext.Instance.GetService<ISubSonicQueryProvider<TEntity>>();

            Expression 
                predicate = null, 
                where = null;

            string[] keys = model.GetPrimaryKey().ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
               predicate = builder.BuildPredicate(predicate, DbExpressionType.Where, keys[i], keyData[i], ComparisonOperator.Equal, GroupOperator.AndAlso);
            }

            where = builder.BuildWhere(typeof(ISubSonicCollection<TEntity>), predicate);

            return (ISubSonicCollection<TEntity>)builder.CreateQuery<TEntity>(builder.BuildSelect(where));
        }
    }
}
