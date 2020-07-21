using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Data.Caching
{
    using Logging;
    using Linq;

    public class ChangeTrackerCollection
        : IEnumerable<KeyValuePair<Type, IEnumerable<IEntityProxy>>>
    {
        private readonly Dictionary<Type, ChangeTrackerElement> collection;

        private DbQueryType dbQueryType;

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
                collection.Add(elementKey, new ChangeTrackerElement<TEntity>(
                    SubSonicContext.DbModel.GetEntityModel<TEntity>(), 
                    SubSonicContext.ServiceProvider.GetService<ISubSonicLogger<ChangeTrackerElement<TEntity>>>()));
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
            if (collection.ContainsKey(elementKey))
            {
                return collection[elementKey].Count(expression);
            }
            else
            {
                return default(int);
            }
        }

        public bool SaveChanges()
        {
            bool result = SaveChanges(out string error_feedback);

            if (!result && error_feedback.IsNotNullOrEmpty())
            {
                var logger = SubSonicContext.ServiceProvider.GetService<ISubSonicLogger<ChangeTrackerCollection>>();

                if (logger.IsNotNull())
                {
                    logger.LogError(error_feedback);
                }
            }

            return result;
        }

        private IEnumerable<IEntityProxy> GetEntitiesToOperateOn(KeyValuePair<Type, IEnumerable<IEntityProxy>> set)
        {
            switch (dbQueryType)
            {
                case DbQueryType.Insert:
                    return set.Value.Where(x => x.IsNew).ToArray();
                case DbQueryType.Update:
                    return set.Value.Where(x => !x.IsNew && x.IsDirty).ToArray();
                case DbQueryType.Delete:
                    return set.Value.Where(x => !x.IsNew && x.IsDeleted).ToArray();
                case DbQueryType.Unknown:
                    break;
            }

            throw Error.NotSupported($"{dbQueryType} is not supported.");
        }

        public bool SaveChanges(out string feedback)
        {
            bool success = true;

            StringBuilder errors = new StringBuilder();

            foreach (DbQueryType queryType in new[] { DbQueryType.Insert, DbQueryType.Update, DbQueryType.Delete })
            {
                dbQueryType = queryType;

                foreach (var dataset in this)
                {
                    var data = GetEntitiesToOperateOn(dataset);
                    
                    string error_feedback = "";

                    if (data.Any())
                    {
                        success &= collection[dataset.Key].SaveChanges(dbQueryType, data, out error_feedback);
                        if (!success && error_feedback.IsNotNullOrEmpty())
                        {
                            errors.AppendLine(error_feedback);
                        }
                    }
                }
            }

            feedback = errors.ToString();

            return success;
        }

        public TResult Where<TResult>(Type elementKey, System.Linq.IQueryProvider provider, Expression expression)
        {
            object result = collection[elementKey].Where(provider, expression);

            if (result is TResult success)
            {
                return success;
            }
            else if (result is IEnumerable<TResult> workneeded)
            {
                return workneeded.FirstOrDefault();
            }
            else
            {
                return default(TResult);
            }
        }

        public IEnumerable Where(Type elementKey, IQueryProvider provider, Expression expression)
        {
            return collection[elementKey].Where(provider, expression);
        }

        private IEnumerable<KeyValuePair<Type, IEnumerable<IEntityProxy>>> BuildEnumeration()
        {
            switch(dbQueryType)
            {
                case DbQueryType.Update:
                case DbQueryType.Insert:
                    return collection
                        .OrderBy(x => x.Value.Model.ObjectGraphWeight)
                        .Select(set => new KeyValuePair<Type, IEnumerable<IEntityProxy>>(set.Key, set.Value.ToProxyCollection()));
                case DbQueryType.Delete:
                    return collection
                        .OrderByDescending(x => x.Value.Model.ObjectGraphWeight)
                        .Select(set => new KeyValuePair<Type, IEnumerable<IEntityProxy>>(set.Key, set.Value.ToProxyCollection()));
                case DbQueryType.Unknown:
                    return collection
                       .Select(set => new KeyValuePair<Type, IEnumerable<IEntityProxy>>(set.Key, set.Value.ToProxyCollection()));
            }
            throw Error.NotSupported();
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
