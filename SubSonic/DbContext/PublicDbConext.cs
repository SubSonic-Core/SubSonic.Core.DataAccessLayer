using System;
using System.Data;

using System.Collections.Generic;

namespace SubSonic
{
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;

    public partial class DbContext
    {
        public DbContextOptions Options { get; }

        public DbModel Model { get; }

        public static DbContext Current => ServiceProvider.GetService<DbContext>();

        public DbSetCollection<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return Instance.GetService<DbSetCollection<TEntity>>();
        }

        public DbDatabase Database => Instance.GetService<DbDatabase>();

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
