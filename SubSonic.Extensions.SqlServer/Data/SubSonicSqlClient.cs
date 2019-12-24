using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using Infrastructure.Factory;
    using System.Data.Common;

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

        public override DbParameter CreateParameter(SubSonicParameter parameter)
        {
            SqlParameter db = (SqlParameter)CreateParameter();

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
