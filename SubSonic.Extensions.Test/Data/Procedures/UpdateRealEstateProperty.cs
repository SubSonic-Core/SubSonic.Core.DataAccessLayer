using SubSonic.Extensions.SqlServer;
using SubSonic.Infrastructure;
using System.Collections.Generic;
using System.Data;

namespace SubSonic.Extensions.Test
{
    [DbStoredProcedure(nameof(UpdateRealEstateProperty))]
    public class UpdateRealEstateProperty
    {
        public UpdateRealEstateProperty(IEnumerable<Models.RealEstateProperty> properties)
        {
            Properties = properties;
        }

        [DbSqlParameter(nameof(Properties), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Structured)]
        public IEnumerable<Models.RealEstateProperty> Properties { get; }
    }
}
