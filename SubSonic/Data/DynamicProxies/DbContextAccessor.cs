using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Data.DynamicProxies
{
    internal class DbContextAccessor
    {
        private readonly DbContext dbContext;

        public DbContextAccessor(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public TType Load<TType>(TType source, PropertyInfo propertyInfo) 
            where TType : class
        {
            throw new NotImplementedException();
        }

        public ICollection<TType> LoadCollection<TType>(TType source, PropertyInfo propertyInfo)
            where TType : class
        {
            throw new NotImplementedException();
        }
    }
}
