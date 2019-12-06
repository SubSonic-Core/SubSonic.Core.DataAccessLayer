using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbModel
    {
        public DbModel()
        {
        }

        public ICollection<DbEntityModel> EntityModels { get; }
    }
}
