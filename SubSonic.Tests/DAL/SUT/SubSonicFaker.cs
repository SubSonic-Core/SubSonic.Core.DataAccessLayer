using Bogus;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL.SUT
{
    class SubSonicFaker<TEntity>
        : Faker<TEntity>
        where TEntity: class
    {
        public SubSonicFaker<TEntity> UseDbContext()
        {
            if (base.CustomInstantiator(f => DbContext.Current.NewEntity<TEntity>()) is SubSonicFaker<TEntity> faker)
            {
                return faker;
            }

            throw Error.InvalidOperation();
        }
    }
}
