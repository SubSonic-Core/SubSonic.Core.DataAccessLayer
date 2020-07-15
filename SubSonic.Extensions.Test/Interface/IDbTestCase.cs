using SubSonic;
using SubSonic.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.Test
{
    public interface IDbTestCase
    {
        IDbEntityModel EntityModel { get; }

        ISubSonicCollection DataSet { get; }

        bool UseDefinedTableType { get; }

        string Expectation { get; }

        void Insert(IEnumerable<IEntityProxy> entities);

        void Update(IEnumerable<IEntityProxy> entities);

        void Delete(IEntityProxy proxy);

        IEntityProxy FindByID(params object[] keyData);

        IEnumerable FetchAll();

        int Count();
    }
}
