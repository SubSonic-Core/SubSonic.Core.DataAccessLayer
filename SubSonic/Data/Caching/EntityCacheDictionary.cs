using SubSonic.Infrastructure;
using SubSonic.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace SubSonic.Data.Caching
{
    public class EntityCacheCollection
        : IEnumerable<IEntityProxy>, IEnumerable
    {
        private readonly Dictionary<Type, ICollection> cache;

        public EntityCacheCollection()
        {
            cache = new Dictionary<Type, ICollection>();
        }

        public ICollection<Type> Keys => cache.Keys;

        public ObservableCollection<IEntityProxy<TEntity>> GetFor<TEntity>()
        {
            Type elementType = typeof(TEntity);

            if (!cache.ContainsKey(elementType))
            {
                Add(elementType, new ObservableCollection<IEntityProxy<TEntity>>());
            }

            return (ObservableCollection<IEntityProxy<TEntity>>)cache[elementType];
        }

        public ObservableCollection<IEntityProxy> GetFor(Type elementType)
        {
            if (!cache.ContainsKey(elementType))
            {
                Add(elementType, new ObservableCollection<IEntityProxy>());
            }

            return (ObservableCollection<IEntityProxy>)cache[elementType];
        }

        protected virtual void Add(Type key, ICollection entities)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            cache.Add(key, entities);
        }

        public void Add(Type elementType, object element)
        {
            ((IList)cache[elementType]).Add(element);
        }

        public void AddRange<TEntity>(Type key, IEnumerable<IEntityProxy<TEntity>> entities)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (cache.ContainsKey(key))
            {
                foreach (IEntityProxy<TEntity> proxy in entities)
                {
                    ((ObservableCollection<IEntityProxy<TEntity>>)cache[key]).Add(proxy);
                }
            }
            else
            {
                cache.Add(key, new ObservableCollection<IEntityProxy<TEntity>>(entities));
            }
            
        }

        public void Clear()
        {
            cache.Clear();
        }

        public IEnumerator<IEntityProxy> GetEnumerator()
        {
            return cache.Values.Select(x => (ObservableCollection<IEntityProxy>)x).SelectMany(x => x).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)cache.Values.Select(x => (ObservableCollection<IEntityProxy>)x).SelectMany(x => x)).GetEnumerator();
        }
    }
}
