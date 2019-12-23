using SubSonic.Extensions.SqlServer;
using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.MockDbClient.Syntax;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SubSonic.Extensions.Test
{
    public class SubSonicMockDbClient
        : SubSonicDbProvider<MockDbClientFactory>
    {
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Provider Factory Pattern")]
        public static readonly SubSonicMockDbClient Instance = new SubSonicMockDbClient();

        protected SubSonicMockDbClient() 
            : base(MockDbClientFactory.Instance)
        {
            QueryProvider = new SqlServerQueryProvider();
        }

        public override ISqlQueryProvider QueryProvider { get; }

        public void ClearBehaviors() => Provider.ClearBehaviors();

        public void AddBehavior(MockCommandBehavior behavior) => Provider.AddBehavior(behavior);

        public override int GetDbType(Type netType, bool unicode = false)
        {
            DbType result;

            if (netType == typeof(int))
            {
                result = DbType.Int32;
            }
            else if (netType == typeof(short))
            {
                result = DbType.Int16;
            }
            else if (netType == typeof(long))
            {
                result = DbType.Int64;
            }
            else if (netType == typeof(DateTime))
            {
                result = DbType.DateTime;
            }
            else if (netType == typeof(float))
            {
                result = DbType.Single;
            }
            else if (netType == typeof(decimal))
            {
                result = DbType.Decimal;
            }
            else if (netType == typeof(double))
            {
                result = DbType.Double;
            }
            else if (netType == typeof(Guid))
            {
                result = DbType.Guid;
            }
            else if (netType == typeof(bool))
            {
                result = DbType.Boolean;
            }
            else if (netType == typeof(byte[]))
            {
                result = DbType.Binary;
            }
            else if (netType == typeof(char))
            {
                result = unicode ? DbType.StringFixedLength : DbType.AnsiStringFixedLength;
            }
            else
            {
                result = unicode ? DbType.String : DbType.AnsiString;
            }

            return (int)result;
        }
    }
}
