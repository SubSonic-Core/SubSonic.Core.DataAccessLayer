using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbContextOptionsBuilder
    {
        private readonly DbContextOptions options;

        public DbContextOptionsBuilder(DbContextOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
