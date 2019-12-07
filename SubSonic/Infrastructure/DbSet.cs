using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    public class DbSet<TEntity>
        : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable, IListSource
        where TEntity : class
    {
        private readonly DbContext dbContext;
        private readonly IQueryProvider provider;
        private readonly DbEntityModel model;
        private readonly List<TEntity> queryableData;
        
        public DbSet(DbContext dbContext, IQueryProvider provider)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            this.queryableData = new List<TEntity>();
            this.model = dbContext.Model.GetEntityModel<TEntity>();
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression => queryableData.AsQueryable().Expression;

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
            DbExpressionBuilder builder = new DbExpressionBuilder(Expression.Parameter(ElementType, ElementType.Name.ToLower()));

            for(int i = 0; i < model.PrimaryKey.Length; i++)
            {
                builder.BuildComparisonExpression(model.PrimaryKey[i], keyData[i], EnumComparisonOperator.Equal, EnumGroupOperator.AndAlso);
            }

            return Provider.CreateQuery<TEntity>(builder.BuildWhereExpression<TEntity>(Expression));
        }
    }
}
