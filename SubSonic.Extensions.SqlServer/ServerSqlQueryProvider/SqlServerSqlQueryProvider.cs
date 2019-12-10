using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.SqlServer.SqlQueryProvider
{
    using Infrastructure;

    public class SqlServerSqlQueryProvider
        : SqlQueryProvider
    {
        public static readonly SqlServerSqlQueryProvider Instance = new SqlServerSqlQueryProvider();

        public SqlServerSqlQueryProvider()
            : base(new SqlServerSqlFragment())
        {

        }

        public override string ClientName => typeof(System.Data.SqlClient.SqlClientFactory).FullName;
    }
}
