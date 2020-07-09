using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SubSonic.Data.Caching
{
    using Collections;
    using Infrastructure;
    using Infrastructure.Logging;
    using Infrastructure.Schema;
    using Linq;
    using Linq.Expressions;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "It is getting to wordy if I do this")]
    public class ChangeTrackerElement<TEntity>
        : ChangeTrackerElement, IEnumerable<TEntity>
    {
        private readonly ISubSonicLogger logger;

        public ChangeTrackerElement(IDbEntityModel dbEntityModel, ISubSonicLogger<ChangeTrackerElement<TEntity>> logger)
            : base(typeof(TEntity)) 
        {
            Model = dbEntityModel ?? throw new ArgumentNullException(nameof(dbEntityModel));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Cache = new ObservableCollection<IEntityProxy<TEntity>>();
        }

        DbDatabase Database => DbContext.Current.Database;

        public ICollection<IEntityProxy<TEntity>> Entities { get; }

        public override void Add(object record)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                if(record is IEntityProxy<TEntity> entity)
                {
                    if (!cache.Any(x => x.IsNew == false && x.KeyData.IsSameAs(entity.KeyData)))
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

                        if (where.GetArgument(1) is Expression<Func<TEntity, bool>> predicate)
                        {
                            results = results.Where(predicate.Compile());
                        }
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

            using (logger.Start(nameof(SaveChanges)))
            {

                bool success = false;

                error = "";

                try
                {
                    IEntityProxy<TEntity>[] result = Array.Empty<IEntityProxy<TEntity>>();

                    using (logger.Start($"On{nameof(SaveChanges)}"))
                    {
                        DbCommandQuery command = Model.Commands[queryType];

                        if (command is null || command.CommandType == CommandType.Text)
                        {
                            switch (queryType)
                            {
                                case DbQueryType.Insert:
                                case DbQueryType.Update:
                                case DbQueryType.Delete:

                                    ISubSonicQueryProvider provider = Database.GetQueryBuilderFor(Model);

                                    if (Model.DefinedTableTypeExists)
                                    {
                                        result = Database
                                            .ExecuteDbQuery<TEntity>(provider, queryType, data, out error)
                                            .Select(x => x as IEntityProxy<TEntity>)
                                            .ToArray();
                                    }
                                    else
                                    {
                                        foreach (IEntityProxy proxy in data)
                                        {
                                            result = result.Union(Database
                                                    .ExecuteDbQuery<TEntity>(provider, queryType, new[] { proxy }, out error)
                                                    .Select(x => x as IEntityProxy<TEntity>))
                                                .ToArray();
                                        }
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else if (command.CommandType == CommandType.StoredProcedure)
                        {
                            if (!Model.DefinedTableTypeExists)
                            {
                                throw new InvalidOperationException(SubSonicErrorMessages.UserDefinedTableNotDefined.Format(Model.EntityModelType.Name));
                            }

                            DbSubSonicStoredProcedure procedure = (DbSubSonicStoredProcedure)Activator.CreateInstance(command.StoredProcedureType, data);

                            result = Database
                                .ExecuteStoredProcedure<TEntity>(procedure)
                                .Select(x => x as IEntityProxy<TEntity>)
                                .ToArray();

                            if (procedure.Result != 0)
                            {
                                // flush the invalid data
                                data.ForEach(x => Remove(x));

                                error = procedure.Error;

                                return success;
                            }
                        }
                    }

                    using (logger.Start($"OnAfter{nameof(SaveChanges)}"))
                    {
                        Action<IEntityProxy<TEntity>> flag = new Action<IEntityProxy<TEntity>>(x =>
                        {
                            x.IsNew = false;
                            x.IsDirty = false;
                            x.IsDeleted = false;
                        });

                        //for(int index = 0, n = data.Count(); index < n; index++)
                        Parallel.For(0, data.Count(), index =>
                        {
                            if (data.ElementAt(index) is IEntityProxy<TEntity> entity)
                            {
                                if (queryType == DbQueryType.Delete)
                                {
                                    lock (Cache)
                                    {
                                        Remove(entity);
                                    }

                                    //continue;
                                    return;
                                }

                                if (result.Length == 0)
                                {
                                    flag(entity);

                                    //continue;
                                    return;
                                }

                                IEntityProxy<TEntity> @object = result[index];

                                if (queryType == DbQueryType.Update)
                                {
                                    @object = result.Single(x =>
                                        x.KeyData.SequenceEqual(entity.KeyData));
                                }

                                if (queryType == DbQueryType.Insert)
                                {
                                    entity.SetKeyData(@object.KeyData);
                                }

                                if (queryType.In(DbQueryType.Insert, DbQueryType.Update))
                                {
                                    entity.SetDbComputedProperties(@object);
                                    flag(entity);
                                }
                            }
                        }
                        );
                    }

                    success = true;
                }
                finally { }

                return success;
            }
        }

        public override IEnumerable Where(IQueryProvider provider, Expression query)
        {
            if (Cache is ObservableCollection<IEntityProxy<TEntity>> cache)
            {
                if (query is DbSelectExpression select)
                {
                    IEnumerable<TEntity> results = cache
                            .Where(x => x.IsNew == false && x.IsDirty == false)
                            .Select(x => x.Data);

                    if (select.Where is DbWhereExpression where)
                    {
                        if (where.GetArgument(1) is Expression<Func<TEntity, bool>> predicate)
                        {
                            results = results.Where(predicate.Compile());
                        }
                    }

                    return (IEnumerable)Activator.CreateInstance(typeof(SubSonicCollection<>).MakeGenericType(Key),
                                provider,
                                query,
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

        public IDbEntityModel Model { get; protected set; }

        public void Clear()
        {
            ((IList)Cache).Clear();
        }

        public abstract void Add(object record);

        public abstract bool Remove(object record);

        public abstract int Count(Expression expression);

        public abstract bool SaveChanges(DbQueryType queryType, IEnumerable<IEntityProxy> data, out string error);

        public abstract IEnumerable Where(IQueryProvider provider, Expression expression);

        public IEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }
    }
}
