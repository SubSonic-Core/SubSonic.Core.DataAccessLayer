using SubSonic.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure
{
#if X86
    using Factories;
#endif
    public class DbDatabase
    {
        private readonly ISubSonicLogger<DbDatabase> logger;

        public DbDatabase(DbProviderFactory dbProviderFactory, ISubSonicLogger<DbDatabase> logger)
        {
            DbProviderFactory = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DbProviderFactory DbProviderFactory { get; }
    }
}
