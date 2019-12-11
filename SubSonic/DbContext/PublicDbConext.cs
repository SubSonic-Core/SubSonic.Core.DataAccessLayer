using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public partial class DbContext
    {
        public DbContextOptions Options { get; }

        public DbModel Model { get; }

        public DbSetCollection<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return Instance.GetService<DbSetCollection<TEntity>>();
        }

        public DbDatabase Database => Instance.GetService<DbDatabase>();

        public IServiceProvider Instance { get; internal set; }

        /// <summary>
        /// Use a shared connection to get data faster and reduce connection overhead
        /// </summary>
        /// <returns></returns>
        public SharedDbConnectionScope UseSharedDbConnection() => Instance.GetService<SharedDbConnectionScope>();
    }
}
