using System;
using System.Data;

using System.Collections.Generic;

namespace SubSonic
{
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;
    using SubSonic.Data.Caching;
    using SubSonic.Data.DynamicProxies;

    /// <summary>
    /// SubSonic Context
    /// </summary>
    public partial class SubSonicContext
    {
        public SubSonicContextOptions Options { get; }

        public SubSonicSchemaModel Model { get; }

        public bool IsDbModelReadOnly { get; private set; }

        public static SubSonicContext Current => ServiceProvider.GetService<SubSonicContext>();

        public ISubSonicSetCollection Set(Type entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Instance.GetService(typeof(ISubSonicSetCollection<>).MakeGenericType(entity)) is ISubSonicSetCollection set)
            {
                return set;
            }

            throw new NotSupportedException();
        }

        public ISubSonicSetCollection<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (Set(typeof(TEntity)) is ISubSonicSetCollection<TEntity> set)
            {
                return set;
            }

            throw new NotSupportedException();
            //return Instance.GetService<DbSetCollection<TEntity>>();
        }

        /// <summary>
        /// Instanciate a new proxy <see cref="DynamicProxy.CreateProxyInstanceOf{TEntity}(SubSonicContext)"/>
        /// </summary>
        /// <typeparam name="TEntity">registered type of <see cref="DbEntityModel"> in the <see cref="SubSonicSchemaModel"></typeparam>
        /// <returns><see cref="DynamicProxy"/> sub class of <see cref="{TEntity}"></returns>
        public TEntity NewEntity<TEntity>()
        {
            return DynamicProxy.CreateProxyInstanceOf<TEntity>(this);
        }

        /// <summary>
        /// Instanciate a new proxy <see cref="DynamicProxy.CreateProxyInstanceOf{TEntity}(SubSonicContext)"/>
        /// </summary>
        /// <typeparam name="TEntity">registered type of <see cref="DbEntityModel"></typeparam>
        /// <param name="entity">instanciated model to be mapped into a proxy</param>
        /// <returns><see cref="DynamicProxy"/> sub class of <see cref="{TEntity}"></returns>
        public TEntity NewEntityFrom<TEntity>(TEntity entity)
        {
            TEntity proxy = DynamicProxy.MapInstanceOf(entity, NewEntity<TEntity>());

            if (proxy is IEntityProxy _proxy)
            {
                _proxy.IsDirty = false;
            }

            return proxy;
        }

        public DbDatabase Database => Instance.GetService<DbDatabase>();
        /// <summary>
        /// Change Tracking implmented by <see cref="ChangeTrackerCollection"/>
        /// </summary>
        public ChangeTrackerCollection ChangeTracking => Instance.GetService<ChangeTrackerCollection>();

        public IServiceProvider Instance => ServiceProvider;

        public bool SaveChanges()
        {
            bool result = false;

            try
            {
                using (SharedDbConnectionScope Scope = UseSharedDbConnection())
                {
                    result = ChangeTracking.SaveChanges();
                }
            }
            finally { }

            return result;
        }

        /// <summary>
        /// Use a shared connection to get data faster and reduce connection overhead
        /// </summary>
        /// <returns></returns>
        public SharedDbConnectionScope UseSharedDbConnection() => Instance.GetService<SharedDbConnectionScope>();
    }
}
