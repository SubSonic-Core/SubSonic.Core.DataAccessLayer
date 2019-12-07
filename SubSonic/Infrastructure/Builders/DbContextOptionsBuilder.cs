using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbContextOptionsBuilder
    {
        private readonly DbContext dbContext;
        private readonly DbContextOptions options;

        private bool isDirtyServiceProvider = false;

        public DbContextOptionsBuilder(DbContext dbContext, DbContextOptions options)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void EnableProxyGeneration()
        {
            options.EnableProxyGeneration = true;
        }
        public void SetServiceProvider(IServiceProvider provider)
        {
            if (!isDirtyServiceProvider)
            {
                dbContext.Instance = provider;
            }
        }
    }
}
