using SubSonic.Extensions.SqlServer;
using SubSonic;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SubSonic.Extensions.Test
{
    [DbStoredProcedure(nameof(DeleteRealEstateProperty), IsNonQuery = true)]
    public class DeleteRealEstateProperty
        : DbSubSonicCommandQueryProcedure<Models.RealEstateProperty>
    {
        public DeleteRealEstateProperty(IEnumerable<IEntityProxy> properties)
            : base(properties) { }
    }
}
