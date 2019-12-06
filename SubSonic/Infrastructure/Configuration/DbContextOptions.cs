using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbContextOptions
    {
        public DbContextOptions()
        {
        }

        public bool EnableProxyGeneration { get; internal set; }
    }
}
