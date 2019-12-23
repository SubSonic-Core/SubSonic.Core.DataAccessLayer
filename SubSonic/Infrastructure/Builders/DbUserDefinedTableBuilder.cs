using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Linq;
    using Schema;

    public class DbUserDefinedTableBuilder
    {
        private class SQL
        {
            public const string UserDefinedTableExists = 
@"SELECT
	COUNT(*)
FROM sys.types 
	JOIN sys.schemas ON types.schema_id = schemas.schema_id
WHERE schemas.name = '{0}' AND types.name = '{1}'";

            public const string CreateDefinedTable = "CREATE TYPE [{0}].[{1}] AS TABLE({2});";

            public static string DropUserDefinedTable = 
$@"IF (EXISTS({UserDefinedTableExists}))
BEGIN
	DROP TYPE [{{0}}].[{{1}}];
END;";

            public static string CreateDefinedTableIfNotExist =
$@"IF (NOT EXISTS({UserDefinedTableExists}))
BEGIN
	{CreateDefinedTable};
END;";

            
        }

        private readonly Type _type;
        private readonly object[] _data;
        private readonly IDbEntityModel _model;

        public DbUserDefinedTableBuilder(params object[] data)
        {
            if(data.Length == 0)
            {
                throw new ArgumentException($"{data.Length} elements found.", nameof(data));
            }

            _data = data;
        }

        public DbUserDefinedTableBuilder(IDbEntityModel model, params object[] data)
            : this(data)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _type = model.EntityModelType;
        }

        public DbUserDefinedTableBuilder(Type type, params object[] data)
            : this(data)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            else if (type.GetCustomAttribute<DbUserDefinedTableTypeAttribute>() == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture ,SubSonicErrorMessages.UserDefinedTableNotDefined, type.Name, typeof(DbUserDefinedTableTypeAttribute).Name));
            }

            _type = type;
        }

        IDbObject Table => _model ?? (IDbObject)_type.GetCustomAttribute<DbUserDefinedTableTypeAttribute>();

        public string GenerateSql()
        {
            StringBuilder oUserDefinedTable = new StringBuilder();
            
            if (_model.IsNull())
            {
                oUserDefinedTable
                    .AppendFormat(CultureInfo.CurrentCulture, SQL.DropUserDefinedTable, Table.SchemaName, Table.Name)
                    .AppendLine()
                    .AppendFormat(CultureInfo.CurrentCulture, SQL.CreateDefinedTable, Table.SchemaName, Table.Name, GenerateSqlBody(GetColumnInformation()));
            }
            else
            {
                oUserDefinedTable
                    .AppendFormat(CultureInfo.CurrentCulture, SQL.CreateDefinedTableIfNotExist, Table.SchemaName, Table.Name, GenerateSqlBody(GetColumnInformation()));
            }

            return oUserDefinedTable.ToString();
        }

        private string GenerateSqlBody(IEnumerable<DbUserDefinedTableColumn> columns)
        {
            StringBuilder oUserDefinedTableBody = new StringBuilder().AppendLine();

            DbUserDefinedTableColumn[] keys = columns.Where(Column => Column.IsPrimaryKey).ToArray();

            for (int x = 0; x < columns.Count(); x++)
            {
                DbUserDefinedTableColumn column = columns.ElementAt(x);

                Type propertyType = column.Property.PropertyType;

                oUserDefinedTableBody.AppendFormat(CultureInfo.CurrentCulture, "\t[{0}] {1} {2}"
                    , column.Name
                    , GenerateDataType(column.DbType, column.Property.PropertyType)
                    , column.IsNullable ? "NULL" : "NOT NULL");

                if (x < columns.Count() - 1)
                {
                    oUserDefinedTableBody
                        .AppendLine(",");
                }
            }

            if (keys.Count() > 0)
            {
                oUserDefinedTableBody
                    .AppendLine(",")
                    .AppendLine("\tPRIMARY KEY CLUSTERED")
                    .AppendLine("\t(")
                    .AppendLine(string.Join(",", keys.Select(key => string.Format(CultureInfo.CurrentCulture, "\t\t[{0}] ASC", key.Name))))
                    .AppendLine("\t) WITH (IGNORE_DUP_KEY = OFF)");
            }

            return oUserDefinedTableBody.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        /// TODO: Re-Visit when this can be done based on configured db factory
        private string GenerateDataType(int dbType, Type propertyType)
        {
            switch ((SqlDbType)dbType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.NText:
                    var attribute = propertyType.GetCustomAttribute<MaxLengthAttribute>();

                    return $"[{(SqlDbType)dbType}]({attribute.IsNotNull(a => a.Length.ToString(CultureInfo.CurrentCulture), "MAX")})";
                case SqlDbType.Decimal:
                    return $"[{(SqlDbType)dbType}](18,2)";
                default:
                    return $"[{(SqlDbType)dbType}]";
            }
        }

        public DataTable GenerateTable()
        {
            DataTable dt = new DataTable();

            IEnumerable<DbUserDefinedTableColumn> columns = GetColumnInformation();

            AddColumns(columns, dt);

            if (_data != null)
            {   // if we have no value we are generating an empty data table for certain
                AddRows(columns, dt);
            }

            return dt;
        }

        private void AddRows(IEnumerable<DbUserDefinedTableColumn> columns, DataTable dt)
        {
            foreach (object obj in _data)
            {
                DataRow row = dt.NewRow();

                foreach (DbUserDefinedTableColumn column in columns)
                {
                    row.SetField(column.Name, column.Property.GetValue(obj));
                }

                dt.Rows.Add(row);
            }
        }

        private void AddColumns(IEnumerable<DbUserDefinedTableColumn> columns, DataTable dt)
        {
            foreach (DbUserDefinedTableColumn column in columns)
            {
                Type type = column.Property.PropertyType;

                dt.Columns.Add(new DataColumn(column.Name, type.GetUnderlyingType())
                {
                    AllowDBNull = column.IsNullable
                });
            }
        }

        private IEnumerable<DbUserDefinedTableColumn> GetColumnInformation()
        {
            if(_model.IsNotNull())
            {
                return GetColumnInformation(_model);
            }

            return GetColumnInformation(_type);
        }

        private IEnumerable<DbUserDefinedTableColumn> GetColumnInformation(Type runtimeType)
        {
            List<DbUserDefinedTableColumn> columns = new List<DbUserDefinedTableColumn>();

            foreach (PropertyInfo info in runtimeType.GetProperties())
            {
                DbUserDefinedTableColumn column = new DbUserDefinedTableColumn(info);

                if(column.Order == -1)
                {
                    column.Order = columns.Count;
                }

                columns.Add(column);
            }

            return columns.ToArray();
        }

        private IEnumerable<DbUserDefinedTableColumn> GetColumnInformation(IDbEntityModel model)
        {
            List<DbUserDefinedTableColumn> columns = new List<DbUserDefinedTableColumn>();

            foreach (IDbEntityProperty property in model.Properties)
            {
                if (property.EntityPropertyType != DbEntityPropertyType.Value)
                {
                    continue;
                }

                DbUserDefinedTableColumn column = new DbUserDefinedTableColumn(_type.GetProperty(property.PropertyName), property);

                columns.Add(column);
            }

            return columns.ToArray();
        }
    }
}
