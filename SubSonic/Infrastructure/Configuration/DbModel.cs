using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbModel
    {
        private readonly DbContext dbContext;

        public DbModel(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            EntityModels = new List<DbEntityModel>();
        }

        public ICollection<DbEntityModel> EntityModels { get; }
    }
}
