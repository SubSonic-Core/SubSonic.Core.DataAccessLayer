using System;
using System.Data.SqlClient;

namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using Infrastructure.SqlGenerator;
    using System;

    public class SqlServerQueryProvider
        : SqlQueryProvider
    {
        public SqlServerQueryProvider()
            : base(CreateSqlContext<AnsiSqlFragment, SqlServerSqlMethods>())
        {

        }

        public override string ClientName => typeof(SqlClientFactory).FullName;
    }
}
