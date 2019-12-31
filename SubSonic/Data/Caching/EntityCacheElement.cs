using SubSonic.Infrastructure;
using SubSonic.Linq;
using SubSonic.Linq.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Data.Caching
{
    public class EntityCacheElementCollection<TEntity>
        : EntityCacheElement, IEnumerable<TEntity>
    {
        public EntityCacheElementCollection()
            : base(typeof(TEntity)) 
        {
            Cache = new ObservableCollection<IEntityProxy<TEntity>>();
        }

        public ICollection<IEntityProxy<TEntity>> Entities { get; }

        public override void Add(object record)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                if(record is IEntityProxy<TEntity> entity)
                {
                    if (cache.Count(x => x.IsNew == false && x.KeyData.IsSameAs(entity.KeyData)) == 0)
                    {
                        cache.Add(entity);
                    }
                    return;
                }
            }

            throw new NotSupportedException();
        }

        public override int Count(Expression expression)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                if (expression is DbSelectExpression select)
                {
                    IEnumerable<TEntity> results = cache
                            .Where(x => x.IsNew == false && x.IsDirty == false)
                            .Select(x => x.Data);

                    if (select.Where is DbWhereExpression where)
                    {
                        results = results.Where((Expression<Func<TEntity, bool>>)where.LambdaPredicate);
                    }

                    return results.Count();
                }
            }

            throw new NotSupportedException();
        }

        public override TResult Where<TResult>(System.Linq.IQueryProvider provider, Expression expression)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                if (expression is DbSelectExpression select)
                {
                    IEnumerable<TEntity> results = cache
                            .Where(x => x.IsNew == false && x.IsDirty == false)
                            .Select(x => x.Data);

                    if (select.Where is DbWhereExpression where)
                    {
                        results = results.Where((Expression<Func<TEntity, bool>>)where.LambdaPredicate);
                    }

                    return (TResult)Activator.CreateInstance(typeof(SubSonicCollection<>).MakeGenericType(Key),
                            provider,
                            expression,
                            results);
                }
            }

            throw new NotSupportedException();
        }

        public new IEnumerator<TEntity> GetEnumerator()
        {
            return ((ICollection<IEntityProxy<TEntity>>)Cache).Select(x => x.Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cache.GetEnumerator();
        }
    }

    public abstract class EntityCacheElement
        : IEnumerable
    {
        protected EntityCacheElement(Type key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public Type Key { get; }

        public ICollection Cache { get; protected set; }

        public void Clear()
        {
            ((IList)Cache).Clear();
        }

        public abstract void Add(object entity);

        public abstract int Count(Expression expression);

        public abstract TResult Where<TResult>(System.Linq.IQueryProvider provider, Expression expression);

        public IEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }
    }
}
