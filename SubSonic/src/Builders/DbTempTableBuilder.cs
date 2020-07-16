using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SubSonic
{
    using Linq;
    using Schema;

    public class DbTempTableBuilder
        : DbProviderBuilder<IDbEntityProperty>
    {
        private readonly IDbEntityModel _model;
        
        private const string create_template = "CREATE TABLE {0}({1});";
        private const string drop_template = "DROP TABLE {0};";

        public DbTempTableBuilder(IDbEntityModel model)
            : base(SubSonicContext.ServiceProvider.GetService<ISqlQueryProvider>())
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public string GenerateDropSql(string name = null)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                drop_template,
                GetTableName(name));
        }

        public override IEnumerable<IDbEntityProperty> GetColumnInformation()
        {
            return _model.Properties.Where(property =>
                property.EntityPropertyType == DbEntityPropertyType.Value);
        }

        public string GenerateSelectSql(string name = null)
        {
            return GenerateSelectSql(name, GetColumnInformation());
        }

        public override string GenerateSelectSql(string name, IEnumerable<IDbEntityProperty> columns)
        {
            return $"SELECT{GenerateSqlBody(false, GetColumnInformation())}\nFROM {GetTableName(name)};";
        }

        public string GenerateSql(string name = null)
        {
            StringBuilder oTempTable = new StringBuilder();

            oTempTable.AppendFormat(
                CultureInfo.CurrentCulture,
                create_template,
                GetTableName(name),
                GenerateSqlBody(true, GetColumnInformation()));

            return oTempTable.ToString();
        }

        public string GetTableName(string name = null)
        {
            if (name.IsNullOrEmpty())
            {
                name = _model.Name;
            }

            return $"#{name}";
        }

        private string GenerateSqlBody(bool definition, IEnumerable<IDbEntityProperty> columns)
        {
            StringBuilder oTempTableBody = new StringBuilder().AppendLine();

            IEnumerable<IDbEntityProperty> keys = columns.Where(property => property.IsPrimaryKey);

            for (int i = 0, cnt = columns.Count(); i < cnt; i++)
            {
                IDbEntityProperty property = columns.ElementAt(i);

                if (property.EntityPropertyType.NotIn(DbEntityPropertyType.Value))
                {
                    continue;
                }

                if (definition)
                {
                    PropertyInfo info = _model.EntityModelType.GetProperty(property.PropertyName);

                    oTempTableBody.Append(string.Format(
                        CultureInfo.CurrentCulture,
                        "\t[{0}] {1} {2} {3}",
                        property.Name,
                        GenerateDataType(property.DbType, info),
                        property.IsNullable ? "NULL" : "NOT NULL",
                        GenerateDefault(info, property.DbType)).TrimEnd());
                }
                else
                {
                    oTempTableBody.Append($"\t[{property.Name}]");
                }

                if (i < (cnt - 1) && 
                    columns.ElementAt(i + 1).EntityPropertyType.In(DbEntityPropertyType.Value))
                {
                    oTempTableBody
                        .AppendLine(",");
                }
            }

            if (definition && keys.Any())
            {
                oTempTableBody
                    .AppendLine(",")
                    .AppendLine("\tPRIMARY KEY CLUSTERED")
                    .AppendLine("\t\t(")
                    .Append("\t\t\t")
                    .AppendLine(string.Join(", ", keys.Select(key => string.Format(CultureInfo.CurrentCulture, "[{0}] ASC", key.Name))).Trim())
                    .AppendLine("\t\t) WITH (IGNORE_DUP_KEY = OFF)");
            }

            return oTempTableBody.ToString();
        }

        public DbParameter CreateParameter(string name, object value, IDbEntityProperty property)
        {
            return DbProvider.CreateParameter(name, value, property);
        }
    }
}
