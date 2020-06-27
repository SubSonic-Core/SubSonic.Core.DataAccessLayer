using System;
using System.Data;

using System.Collections.Generic;

namespace SubSonic
{
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;
    using SubSonic.Data.Caching;

    public partial class DbContext
    {
        public DbContextOptions Options { get; }

        public DbModel Model { get; }

        public bool IsDbModelReadOnly { get; private set; }

        public static DbContext Current => ServiceProvider.GetService<DbContext>();

        public ISubSonicDbSetCollection Set(Type entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Instance.GetService(typeof(ISubSonicDbSetCollection<>).MakeGenericType(entity)) is ISubSonicDbSetCollection set)
            {
                return set;
            }

            throw new NotSupportedException();
        }

        public ISubSonicDbSetCollection<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (Set(typeof(TEntity)) is ISubSonicDbSetCollection<TEntity> set)
            {
                return set;
            }

            throw new NotSupportedException();
            //return Instance.GetService<DbSetCollection<TEntity>>();
        }

        public DbDatabase Database => Instance.GetService<DbDatabase>();

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
