using System;
using System.Collections.Generic;
using System.Text;
using SubSonic.Infrastructure;

namespace SubSonic.Tests
{
    public class TestDbContext
        : DbContext
    {
        public TestDbContext()
            : base()
        {

        }

        protected override void OnDbConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnDbConfiguring(optionsBuilder);
        }

        protected override void OnDbModeling(DbModelBuilder modelBuilder)
        {
            base.OnDbModeling(modelBuilder);
        }
    }
}
