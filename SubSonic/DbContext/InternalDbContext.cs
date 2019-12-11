using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic
{
    public partial class DbContext
    {
        internal Func<DbConnectionStringBuilder, DbContextOptions, string> GetConnectionString { get; set; }
    }
}
