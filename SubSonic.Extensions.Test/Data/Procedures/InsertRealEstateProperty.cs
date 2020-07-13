using SubSonic.Extensions.SqlServer;
using SubSonic;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SubSonic.Extensions.Test
{
    [DbStoredProcedure(nameof(InsertRealEstateProperty))]
    public class InsertRealEstateProperty
        : DbSubSonicCommandQueryProcedure<Models.RealEstateProperty>
    {
        public InsertRealEstateProperty(IEnumerable<IEntityProxy> properties)
            : base(properties)
        {

        }
    }
}
