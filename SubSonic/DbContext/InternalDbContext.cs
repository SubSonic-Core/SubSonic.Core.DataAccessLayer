using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic
{
    public partial class DbContext
    {
        internal static IServiceProvider ServiceProvider { get; set; }
        internal Func<DbConnectionStringBuilder, DbContextOptions, string> GetConnectionString { get; set; }
    }
}
