using SubSonic.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
#if X86
    using Factories;
#endif
    public class DbDatabase
    {
        private readonly ISubSonicLogger<DbDatabase> logger;
        private readonly DbContext dbContext;
        private readonly DbProviderFactory dbProvider;

        public DbDatabase(ISubSonicLogger<DbDatabase> logger, DbContext dbContext, DbProviderFactory dbProviderFactory)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.dbProvider = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
        }

        internal TResult ExecuteQuery<TResult>(Expression expression)
        {
            using (IPerformanceLogger<DbDatabase> performance = logger.Start($"{nameof(ExecuteQuery)}<{typeof(TResult).GetQualifiedTypeName()}>"))
            {
                throw new NotImplementedException();
            }
        }
    }
}
