using SubSonic.Extensions.SqlServer;
using SubSonic;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SubSonic.Extensions.Test
{
    [DbStoredProcedure(nameof(UpdateRealEstateProperty))]
    public class UpdateRealEstateProperty
        : DbSubSonicCommandQueryProcedure<Models.RealEstateProperty>
    {
        public UpdateRealEstateProperty(IEnumerable<IEntityProxy> properties)
            : base(properties) { }
    }
}
