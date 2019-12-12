namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using Infrastructure.SqlGenerator;
    using System;

    public class SqlServerSqlQueryProvider
        : SqlQueryProvider
    {
        public static readonly SqlServerSqlQueryProvider Instance = new SqlServerSqlQueryProvider();

        public SqlServerSqlQueryProvider()
            : base(SqlQueryProvider.CreateContext<AnsiSqlFragment, SqlServerSqlMethods>())
        {

        }

        public override string ClientName => typeof(System.Data.SqlClient.SqlClientFactory).FullName;

        public override ISqlGenerator BuildSelectStatement()
        {
            throw new NotImplementedException();
        }
    }
}
