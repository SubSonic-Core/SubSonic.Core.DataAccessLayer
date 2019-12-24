using SubSonic.Extensions.SqlServer;
using SubSonic.Extensions.Test.MockDbClient;
using SubSonic.Extensions.Test.MockDbClient.Syntax;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Factory;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

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

        public override DbParameter CreateParameter(SubSonicParameter parameter)
        {
            DbParameter db = CreateParameter();

            db.Map(parameter);

            return db;
        }

        public override DbParameter CreateStoredProcedureParameter(string name, object value, bool mandatory, int size, bool isUserDefinedTableParameter, string udtType, ParameterDirection direction)
        {
            var sqlParameter = new SqlParameter("@" + name, value ?? DBNull.Value)
            {
                Direction = direction,
                IsNullable = !mandatory,
                Size = size,
            };

            if (isUserDefinedTableParameter)
            {
                sqlParameter.TypeName = udtType;
            }
            //else
            //{
            //    sqlParameter.SqlDbType = (SqlDbType)dbType;
            //}

            return sqlParameter;
        }
    }
}
