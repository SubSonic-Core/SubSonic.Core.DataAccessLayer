using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;
    using SubSonic.Linq;
    using System.Reflection;

    public class DbTempTableBuilder
        : DbProviderBuilder
    {
        private readonly IDbEntityModel _model;
        
        private const string create_template = "CREATE TABLE {0}({1});";
        private const string drop_template = "DROP TABLE {0};";

        public DbTempTableBuilder(IDbEntityModel model)
            : base(DbContext.ServiceProvider.GetService<ISqlQueryProvider>())
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

        public string GenerateSelectSql(string name = null)
        {
            return $"SELECT{GenerateSqlBody(false)}\nFROM {GetTableName(name)};";
        }

        public string GenerateSql(string name = null)
        {
            StringBuilder oTempTable = new StringBuilder();

            oTempTable.AppendFormat(
                CultureInfo.CurrentCulture,
                create_template,
                GetTableName(name),
                GenerateSqlBody(true));

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

        private string GenerateSqlBody(bool definition)
        {
            StringBuilder oTempTableBody = new StringBuilder().AppendLine();

            IEnumerable<IDbEntityProperty> keys = _model.Properties.Where(property => property.IsPrimaryKey);

            for (int i = 0, cnt = _model.Properties.Count; i < cnt; i++)
            {
                IDbEntityProperty property = _model.Properties.ElementAt(i);

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
                    _model.Properties.ElementAt(i + 1).EntityPropertyType.In(DbEntityPropertyType.Value))
                {
                    oTempTableBody
                        .AppendLine(",");
                }
            }

            if (definition && keys.Count() > 0)
            {
                oTempTableBody
                    .AppendLine(",")
                    .AppendLine("\tPRIMARY KEY CLUSTERED")
                    .AppendLine("\t\t(")
                    .AppendLine(string.Join(",", keys.Select(key => string.Format(CultureInfo.CurrentCulture, "\t\t\t[{0}] ASC", key.Name))))
                    .AppendLine("\t\t) WITH (IGNORE_DUP_KEY = OFF)");
            }

            return oTempTableBody.ToString();
        }

    }
}
