using Microsoft.Extensions.Logging;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Logging;
using SubSonic.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Data.Caching
{
    public class ChangeTrackerCollection
        : IEnumerable<KeyValuePair<Type, IEnumerable<IEntityProxy>>>
    {
        private readonly Dictionary<Type, ChangeTrackerElement> collection;

        public ChangeTrackerCollection()
        {
            collection = new Dictionary<Type, ChangeTrackerElement>();
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
                collection.Add(elementKey, new ChangeTrackerElement<TEntity>());
            }

            return GetCacheElementFor<TEntity>();
        }

        public void Add(Type elementKey, object entity)
        {
            collection[elementKey].Add(entity);
        }

        public bool Remove(Type elementKey, object entity)
        {
            return collection[elementKey].Remove(entity);
        }

        public void Flush()
        {
            foreach(var entity in collection)
            {
                entity.Value.Clear();
            }
        }

        public int Count(Type elementKey, Expression expression)
        {
            return collection[elementKey].Count(expression);
        }

        public bool SaveChanges()
        {
            bool result = SaveChanges(out string error_feedback);

            if (!result && error_feedback.IsNotNullOrEmpty())
            {
                var logger = DbContext.ServiceProvider.GetService<ISubSonicLogger<ChangeTrackerCollection>>();

                if (logger.IsNotNull())
                {
                    logger.LogError(error_feedback);
                }
            }

            return result;
        }

        public bool SaveChanges(out string feedback)
        {
            bool success = true;

            StringBuilder errors = new StringBuilder();

            foreach (var dataset in this)
            {
                var insert = dataset.Value.Where(x => x.IsNew).ToArray();
                var update = dataset.Value.Where(x => !x.IsNew && x.IsDirty).ToArray();
                var delete = dataset.Value.Where(x => !x.IsNew && x.IsDeleted).ToArray();

                string error_feedback = "";

                if (insert.Count() > 0)
                {
                    success &= collection[dataset.Key].SaveChanges(DbQueryType.Insert, insert, out error_feedback);
                    if (!success && error_feedback.IsNotNullOrEmpty())
                    {
                        errors.AppendLine(error_feedback);

                        error_feedback = "";
                    }
                }

                if (update.Count() > 0)
                {
                    success &= collection[dataset.Key].SaveChanges(DbQueryType.Update, update, out error_feedback);
                    if (!success && error_feedback.IsNotNullOrEmpty())
                    {
                        errors.AppendLine(error_feedback);

                        error_feedback = "";
                    }
                }

                if (delete.Count() > 0)
                {
                    success &= collection[dataset.Key].SaveChanges(DbQueryType.Delete, delete, out error_feedback);
                    if (!success && error_feedback.IsNotNullOrEmpty())
                    {
                        errors.AppendLine(error_feedback);

                        error_feedback = "";
                    }
                }
            }

            feedback = errors.ToString();

            return success;
        }

        public TResult Where<TResult>(Type elementKey, System.Linq.IQueryProvider provider, Expression expression)
        {
            return collection[elementKey].Where<TResult>(provider, expression);
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
