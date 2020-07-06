using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    using Data.Caching;
    using Data.DynamicProxies;
    using Linq;
    using Linq.Expressions;
    using Schema;

    public class DbSetCollection<TEntity>
        : ISubSonicDbSetCollection<TEntity>
        , ISubSonicDbSetCollection
    {
        private readonly IQueryProvider provider;
        private readonly IDbEntityModel model;
        private readonly ICollection<IEntityProxy<TEntity>> dataset;
        
        public DbSetCollection(ISubSonicQueryProvider<TEntity> provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            var dataset = DbContext.ChangeTracking.GetCacheFor<TEntity>();
                
            dataset.CollectionChanged += OnDbSetCollectionChanged;

            this.dataset = dataset;

            model = DbContext.Model.GetEntityModel<TEntity>();
            Expression = DbExpression.DbSelect(this, GetType(), model.Table);
        }

        private void OnDbSetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        protected DbContext DbContext => DbContext.ServiceProvider.GetService<DbContext>();

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider => provider;

        #region ICollection<TEntity> Implementation
        void ISubSonicDbSetCollection.Add(object entity)
        {
            if (entity is TEntity item)
            {
                Add(item);

                return;
            }

            throw new NotSupportedException();
        }

        public void Add(TEntity item)
        {
            foreach(IDbEntityProperty property in model.Properties.Where(x =>
                x.EntityPropertyType == DbEntityPropertyType.Navigation))
            {
                object navigation = model.EntityModelType.GetProperty(property.PropertyName).GetValue(item);

                if (!(navigation is null) && 
                    !(navigation is IEntityProxy))
                {
                    DbContext.Set(property.PropertyType).Add(navigation);
                }
            }

            if (item is IEntityProxy<TEntity> entity)
            {
                dataset.Add(entity);
            }
            else
            {
                IEntityProxy<TEntity> @new = new Entity<TEntity>(item)
                {
                    IsNew = true
                };

                dataset.Add(@new);
            }
        }

        public void AddRange(IEnumerable<TEntity> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (TEntity item in items)
            {
                Add(item);
            }
        }

        public bool Remove(TEntity item)
        {
            return Delete(item);
        }

        public bool Contains(TEntity item)
        {
            if (item is IEntityProxy<TEntity> entity)
            {
                return dataset.Contains(entity);
            }
            else
            {
                return dataset.Contains(new Entity<TEntity>(item));
            }
        }

        public void CopyTo(TEntity[] entities, int startAt)
        {
            dataset.Select(x => x.Data).ToList().CopyTo(entities, startAt);
        }

        public void Clear() => dataset.Clear();

        public int Count => dataset.Count;

        public bool IsReadOnly => false;

        public IEnumerator GetEnumerator()
        {
            if (dataset.Count == 0)
            {
                Load();
            }
            return ((IEnumerable)dataset).GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            if (dataset.Count == 0)
            {
                Load();
            }

            return dataset.Select(x => DynamicProxy.MapInstanceOf(DbContext, x)).GetEnumerator();
        }

        public IQueryable<TEntity> Load()
        {
            SubSonicQueryable.Load(this);

            return this;
        }
        #endregion

        public bool Delete(TEntity entity)
        {
            if (entity is IEntityProxy<TEntity> proxy)
            {
                proxy.IsDeleted = true;

                return proxy.IsDeleted;
            }

            return false;
        }

        public TEntity FindByID(params object[] keyData)
        {
            return FindByID(keyData, model.GetPrimaryKey().ToArray()).Single();
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

                return builder.CreateQuery<TEntity>(
                    builder.BuildSelect(select,
                    builder.BuildWhereFindByIDPredicate(select, keyData, keyNames)));
            }

            throw new NotSupportedException();
        }
    }
}

