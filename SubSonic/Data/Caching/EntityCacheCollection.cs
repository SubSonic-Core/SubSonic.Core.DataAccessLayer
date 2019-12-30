using SubSonic.Infrastructure;
using SubSonic.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Data.Caching
{
    public class EntityCacheCollection
        : IEnumerable<KeyValuePair<Type, IEnumerable<IEntityProxy>>>
    {
        private readonly Dictionary<Type, EntityCacheElement> collection;

        public EntityCacheCollection()
        {
            collection = new Dictionary<Type, EntityCacheElement>();
        }

        protected ObservableCollection<IEntityProxy<TEntity>> GetCacheElementFor<TEntity>()
        {
            Type elementKey = typeof(TEntity);

            if (collection.ContainsKey(elementKey) &&
                collection[elementKey].Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                return cache;
            }

            return null;
        }

        public ObservableCollection<IEntityProxy<TEntity>> GetCacheFor<TEntity>()
        {
            Type elementKey = typeof(TEntity);

            if (!collection.ContainsKey(elementKey))
            {
                collection.Add(elementKey, new EntityCacheElementCollection<TEntity>());
            }

            return GetCacheElementFor<TEntity>();
        }

        public void Add(Type elementKey, object entity)
        {
            if (collection[elementKey].Cache is IList list)
            {
                if (!list.Contains(entity))
                {
                    list.Add(entity);
                }
            }
        }

        public TResult Where<TResult>(Type elementKey, Expression expression)
        {
            return collection[elementKey].Where<TResult>(expression);
        }

        private IEnumerable<KeyValuePair<Type, IEnumerable<IEntityProxy>>> BuildEnumeration()
        {
            List<KeyValuePair<Type, IEnumerable<IEntityProxy>>> enumeration = new List<KeyValuePair<Type, IEnumerable<IEntityProxy>>>();

            foreach(var element in collection)
            {
                enumeration.Add(new KeyValuePair<Type, IEnumerable<IEntityProxy>>(element.Key, element.Value.Select(x => (IEntityProxy)x)));
            }

            return enumeration;
        }

        public IEnumerator<KeyValuePair<Type, IEnumerable<IEntityProxy>>> GetEnumerator()
        {
            return BuildEnumeration().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return BuildEnumeration().GetEnumerator();
        }
    }
}
