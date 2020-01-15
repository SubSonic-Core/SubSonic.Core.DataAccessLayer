using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SubSonic.Extensions.SqlServer
{
    using Infrastructure;
    using Infrastructure.Factory;
    using SubSonic.Infrastructure.Schema;
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

        public override DbDataAdapter CreateDataAdapter(DbCommand select)
        {
            DbDataAdapter adapter = CreateDataAdapter();

            adapter.SelectCommand = select;

            return adapter;
        }

        public override DbParameter CreateParameter(SubSonicParameter parameter)
        {
            SqlParameter db = (SqlParameter)CreateParameter();

            db.Map(parameter);

            return db;
        }

        public override DbParameter CreateSubSonicParameter(string name, object value, IDbEntityProperty property)
        {
            if (property is null)
            {
                return new SubSonicSqlParameter(name, value);
            }
            else
            {
                return new SubSonicSqlParameter(name, value, property);
            }
        }

        public override DbParameter CreateParameter(string name, object value)
        {
            return CreateSubSonicParameter(name, value, null);
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
