using System.Data.SqlClient;

namespace SubSonic.Extensions.SqlServer
{
    using SqlGenerator;

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
