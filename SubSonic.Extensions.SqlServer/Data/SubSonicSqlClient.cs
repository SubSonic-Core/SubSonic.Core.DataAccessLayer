using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using Infrastructure.Factory;

    public class SubSonicSqlClient
        : SubSonicDbProvider<SqlClientFactory>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Provider Factory Pattern")]
        public static readonly SubSonicSqlClient Instance = new SubSonicSqlClient();

        protected SubSonicSqlClient() 
            : base(SqlClientFactory.Instance)
        {
            QueryProvider = new SqlServerQueryProvider();
        }

        public override ISqlQueryProvider QueryProvider { get; }

        public override int GetDbType(Type netType, bool unicode = false)
        {
            if (netType is null)
            {
                throw new ArgumentNullException(nameof(netType));
            }

            SqlDbType result = SqlDbType.Variant;

            // filter down to non nullable types
            netType = netType.GetUnderlyingType();

            if (netType == typeof(int))
            {
                result = SqlDbType.Int;
            }
            else if (netType == typeof(short))
            {
                result = SqlDbType.SmallInt;
            }
            else if (netType == typeof(long))
            {
                result = SqlDbType.BigInt;
            }
            else if (netType == typeof(DateTime))
            {
                result = SqlDbType.DateTime;
            }
            else if (netType == typeof(float))
            {
                result = SqlDbType.Real;
            }
            else if (netType == typeof(decimal))
            {
                result = SqlDbType.Decimal;
            }
            else if (netType == typeof(double))
            {
                result = SqlDbType.Float;
            }
            else if (netType == typeof(Guid))
            {
                result = SqlDbType.UniqueIdentifier;
            }
            else if (netType == typeof(bool))
            {
                result = SqlDbType.Bit;
            }
            else if (netType == typeof(byte))
            {
                result = SqlDbType.TinyInt;
            }
            else if (netType == typeof(byte[]))
            {
                result = SqlDbType.Binary;
            }
            else if (netType == typeof(string))
            {
                result = unicode ? SqlDbType.NVarChar : SqlDbType.VarChar;
            }
            else if (netType == typeof(char))
            {
                result = unicode ? SqlDbType.NChar : SqlDbType.Char;
            }
            else if (netType.IsSubclassOf(typeof(object)))
            {
                result = SqlDbType.Structured;
            }

            return (int)result;
        }
    }
}
