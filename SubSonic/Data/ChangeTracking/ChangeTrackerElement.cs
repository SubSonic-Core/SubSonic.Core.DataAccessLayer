using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using SubSonic.Linq;
using SubSonic.Linq.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Data.Caching
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "It is getting to wordy if I do this")]
    public class ChangeTrackerElement<TEntity>
        : ChangeTrackerElement, IEnumerable<TEntity>
    {
        public ChangeTrackerElement()
            : base(typeof(TEntity)) 
        {
            Cache = new ObservableCollection<IEntityProxy<TEntity>>();
        }

        private static DbDatabase Database => DbContext.Current.Database;

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

        public override bool Remove(object record)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                if (record is IEntityProxy<TEntity> entity)
                {
                    return cache.Remove(entity);
                }
            }

            throw new NotSupportedException();
        }

        public override int Count(Expression expression)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                IEnumerable<TEntity> results = cache
                            .Where(x => x.IsNew == false && x.IsDirty == false)
                            .Select(x => x.Data);

                if (expression is DbSelectExpression select)
                {
                    if (select.Where is DbWhereExpression where)
                    {
                        if (!where.CanReadFromCache)
                        {
                            return 0;
                        }

                        results = results.Where((Expression<Func<TEntity, bool>>)where.LambdaPredicate);
                    }

                    return results.Count();
                }
            }

            throw new NotSupportedException();
        }

        public override bool SaveChanges(DbQueryType queryType, IEnumerable<IEntityProxy> data, out string error)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            bool success = false;

            error = "";

            try
            {
                IDbEntityModel model = DbContext.DbModel.GetEntityModel<TEntity>();

                DbCommandQuery command = model.Commands[queryType];

                IEntityProxy<TEntity>[] result = null;

                if (command is null || command.CommandType == CommandType.Text)
                {
                    switch (queryType)
                    {
                        case DbQueryType.Insert:
                        case DbQueryType.Update:
                        case DbQueryType.Delete:
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (command.CommandType == CommandType.StoredProcedure)
                {
                    DbSubSonicStoredProcedure procedure = (DbSubSonicStoredProcedure)Activator.CreateInstance(command.StoredProcedureType, data);

                    result = Database.ExecuteStoredProcedure<TEntity>(procedure).Select(x => x as IEntityProxy<TEntity>).ToArray();

                    if (procedure.Result != 0)
                    {
                        // flush the invalid data
                        data.ForEach(x => Remove(x));

                        error = procedure.Error;

                        return success;
                    }
                }

                for(int i = 0, n = data.Count(); i < n; i++)
                {
                    if (data.ElementAt(i) is IEntityProxy<TEntity> entity)
                    {
                        if (queryType == DbQueryType.Delete)
                        {
                            Remove(entity);

                            continue;
                        }

                        if(queryType == DbQueryType.Insert)
                        {
                            entity.SetKeyData(result[i].KeyData);
                        }

                        if(queryType.In(DbQueryType.Insert, DbQueryType.Update))
                        {
                            entity.IsNew = false;
                            entity.IsDirty = false;
                            entity.IsDeleted = false;
                        }
                    }
                }

                success = true;
            }
            finally { }

            return success;
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

                    if (typeof(TResult).IsEnumerable())
                    {
                        return (TResult)Activator.CreateInstance(typeof(SubSonicCollection<>).MakeGenericType(Key),
                                provider,
                                expression,
                                results);
                    }
                    else
                    {
                        return results.SingleOrDefault<TEntity, TResult>();
                    }
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Generic inteface is implemented at a higher level.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "handled through inheritance")]
    public abstract class ChangeTrackerElement
        : IEnumerable
    {
        protected ChangeTrackerElement(Type key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public Type Key { get; }

        public ICollection Cache { get; protected set; }

        public void Clear()
        {
            ((IList)Cache).Clear();
        }

        public abstract void Add(object record);

        public abstract bool Remove(object record);

        public abstract int Count(Expression expression);

        public abstract bool SaveChanges(DbQueryType queryType, IEnumerable<IEntityProxy> data, out string error);

        public abstract TResult Where<TResult>(System.Linq.IQueryProvider provider, Expression expression);

        public IEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }
    }
}
