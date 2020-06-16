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
    using System.Data.Common;

    public class DbUserDefinedTableBuilder
        : DbProviderBuilder<DbUserDefinedTableColumn>
    {
        private class SQL
        {
            public const string UserDefinedTableExists = 
@"SELECT
	COUNT(*)
FROM sys.types 
	JOIN sys.schemas ON types.schema_id = schemas.schema_id
WHERE schemas.name = '{0}' AND types.name = '{1}'";

            public const string CreateDefinedTable = "CREATE TYPE [{0}].[{1}] AS TABLE({2})";

            public static string DropUserDefinedTable = 
$@"IF (({UserDefinedTableExists}) > 0)
BEGIN
	DROP TYPE [{{0}}].[{{1}}];
END;";

            public static string CreateDefinedTableIfNotExist =
$@"IF (({UserDefinedTableExists}) = 0)
BEGIN
	{CreateDefinedTable};
END;";

            
        }

        private readonly Type _type;
        private readonly IEnumerable _data;
        private readonly IDbEntityModel _model;

        public DbUserDefinedTableBuilder(IEnumerable data)
            : base(DbContext.ServiceProvider.GetService<ISqlQueryProvider>())
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public DbUserDefinedTableBuilder(IDbEntityModel model, IEnumerable data)
            : this(data)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _type = model.EntityModelType;

            if(!_model.DefinedTableTypeExists)
            {
                throw new InvalidOperationException(SubSonicErrorMessages.UserDefinedTableNotDefined.Format(_type.Name));
            }
        }

        public DbUserDefinedTableBuilder(Type type, IEnumerable data)
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

        IDbObject Table => _model?.DefinedTableType ?? (IDbObject)_type.GetCustomAttribute<DbUserDefinedTableTypeAttribute>();

        public string GenerateSql()
        {
            StringBuilder oUserDefinedTable = new StringBuilder();
            
            if (_model.IsNull())
            {
                oUserDefinedTable
                    .AppendFormat(CultureInfo.CurrentCulture, SQL.DropUserDefinedTable, Table.SchemaName, Table.Name)
                    .AppendLine()
                    .AppendFormat(CultureInfo.CurrentCulture, SQL.CreateDefinedTable, Table.SchemaName, Table.Name, GenerateSqlBody(true, GetColumnInformation()));
            }
            else
            {
                oUserDefinedTable
                    .AppendFormat(CultureInfo.CurrentCulture, SQL.CreateDefinedTableIfNotExist, Table.SchemaName, Table.Name, GenerateSqlBody(true, GetColumnInformation()));
            }

            return oUserDefinedTable.ToString();
        }

        private string GenerateSqlBody(bool definition, IEnumerable<DbUserDefinedTableColumn> columns)
        {
            StringBuilder oUserDefinedTableBody = new StringBuilder().AppendLine();

            DbUserDefinedTableColumn[] keys = columns.Where(Column => Column.IsPrimaryKey).ToArray();

            for (int x = 0, cnt = columns.Count(); x < cnt; x++)
            {
                DbUserDefinedTableColumn column = columns.ElementAt(x);

                if (definition)
                {
                    oUserDefinedTableBody.Append(string.Format(CultureInfo.CurrentCulture, "\t\t[{0}] {1} {2} {3}"
                        , column.Name
                        , GenerateDataType(column.DbType, column.Property)
                        , column.IsNullable ? "NULL" : "NOT NULL"
                        , GenerateDefault(column.Property, column.DbType)).TrimEnd());
                }
                else
                {
                    oUserDefinedTableBody.Append($"\t[{column.Name}]");
                }

                if (x < (cnt - 1))
                {
                    oUserDefinedTableBody
                        .AppendLine(",");
                }
            }

            if (definition && keys.Count() > 0)
            {
                oUserDefinedTableBody
                    .AppendLine(",")
                    .AppendLine("\t\tPRIMARY KEY CLUSTERED")
                    .AppendLine("\t\t(")
                    .Append("\t\t\t")
                    .AppendLine(string.Join(", ", keys.Select(key => string.Format(CultureInfo.CurrentCulture, "[{0}] ASC", key.Name))).Trim())
                    .Append("\t\t) WITH (IGNORE_DUP_KEY = OFF)");
            }

            return oUserDefinedTableBody.ToString();
        }

        public string GenerateSelectSql(string name)
        {
            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name));
            }

            return GenerateSelectSql(name, GetColumnInformation());
        }

        public override string GenerateSelectSql(string name, IEnumerable<DbUserDefinedTableColumn> columns)
        {
            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name));
            }

            return $"SELECT{GenerateSqlBody(false, columns)}\nFROM {name};";
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
                    if (column.Property is null)
                    {
                        continue;
                    }

                    row.SetField(column.Name, column.Property.GetValue(obj) ?? DBNull.Value);
                }

                dt.Rows.Add(row);
            }
        }

        private static void AddColumns(IEnumerable<DbUserDefinedTableColumn> columns, DataTable dt)
        {
            foreach (DbUserDefinedTableColumn column in columns)
            {
                if (column.Property is null)
                {
                    continue;
                }

                Type type = column.PropertyType;

                dt.Columns.Add(new DataColumn(column.Name, type.GetUnderlyingType())
                {
                    AllowDBNull = column.IsNullable
                });
            }
        }

        public override IEnumerable<DbUserDefinedTableColumn> GetColumnInformation()
        {
            if(_model.IsNotNull())
            {
                return GetColumnInformation(_model);
            }

            return GetColumnInformation(_type);
        }

        private static IEnumerable<DbUserDefinedTableColumn> GetColumnInformation(Type runtimeType)
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
                if (property.EntityPropertyType.NotIn(DbEntityPropertyType.Value, DbEntityPropertyType.DAL))
                {
                    continue;
                }

                PropertyInfo info = null;

                if(property.PropertyName.IsNotNullOrEmpty())
                {
                    info = _type.GetProperty(property.PropertyName);
                }

                DbUserDefinedTableColumn column = new DbUserDefinedTableColumn(info, property, model.Commands.DisableKeysForDefinedTableTypes);

                columns.Add(column);
            }

            return columns.ToArray();
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return DbProvider.CreateParameter(name, value, false, 0, true, Table.QualifiedName, ParameterDirection.Input);
        }
    }
}
