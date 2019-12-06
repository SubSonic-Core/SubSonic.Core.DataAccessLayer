using SubSonic.Infrastructure.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbSet<TEntity>
        : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable, IListSource
        where TEntity : class
    {
        private readonly DbContext dbContext;
        private readonly DbEntityModel model;
        private readonly List<TEntity> queryableData;
        
        public DbSet(DbContext dbContext)
        {
            this.dbContext = dbContext;
            this.queryableData = new List<TEntity>();
            this.model = dbContext.Model.GetEntityModel<TEntity>();
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; private set; }

        public IQueryProvider Provider => new SubSonicQueryProvider(dbContext);

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

        public DbSet<TEntity> FindByID(params object[] keyData)
        {
            DbExpressionBuilder builder = new DbExpressionBuilder(Expression.Parameter(ElementType, ElementType.Name.ToLower()));

            for(int i = 0; i < model.PrimaryKey.Length; i++)
            {
                builder.BuildComparisonExpression(model.PrimaryKey[i], keyData[i], EnumComparisonOperator.Equal, EnumGroupOperator.AndAlso);
            }

            Expression = builder.BuildWhereExpression<TEntity>(queryableData.AsQueryable().Expression);

            return this;
        }
    }
}
