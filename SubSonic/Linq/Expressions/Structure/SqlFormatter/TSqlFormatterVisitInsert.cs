using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected internal override Expression VisitInsert(DbInsertExpression insert)
        {
            if (insert.IsNotNull())
            {
                if (insert.Table.Model.DefinedTableTypeExists)
                {
                    FormatInsertWithUserDefinedTableType(insert);
                }
                else
                {
                    FormatInsertWithTempTable(insert);
                }

                return insert;
            }

            return null;
        }

        private void FormatInsertWithTempTable(DbInsertExpression insert)
        {
            DbTempTableBuilder builder = new DbTempTableBuilder(insert.Table.Model);

            WriteNewLine(builder.GenerateSql());

            WriteNewLine($"{Fragments.INSERT_INTO} {insert.Table.QualifiedName}");
            WriteNewLine($"{Fragments.OUTPUT_INSERTED_INTO} {builder.GetTableName()}");
            WriteNewLine(Fragments.VALUES);

            WriteNewLine(Indentation.Inner);

            for(int i = 0, cnt = insert.Values.Count(); i < cnt; i++)
            {
                Write(Fragments.LEFT_PARENTHESIS);
                IEnumerable<IDbEntityProperty> properties = insert.Table.Model.Properties;

                for (int x = 0, x_cnt = properties.Count(); x < x_cnt; x++)
                {
                    IDbEntityProperty property = properties.ElementAt(x);

                    if (property.IsReadOnly || property.EntityPropertyType != DbEntityPropertyType.Value)
                    {
                        continue;
                    }

                    PropertyInfo propertyInfo = insert.Type.GetProperty(property.PropertyName);

                    object element = insert.Values.ElementAt(i);

                    string parameterName = $"@{property.Name}_{i + 1}";

                    object value = propertyInfo.GetValue(element);

                    insert.DbParameters.Add(builder.CreateParameter(
                        parameterName,
                        value ?? DBNull.Value,
                        property));

                    Write(parameterName);

                    if (x < (x_cnt - 1) &&
                        !properties.ElementAt(x + 1).IsReadOnly)
                    {
                        Write($"{Fragments.COMMA} ");
                    }
                }
                Write(Fragments.RIGHT_PARENTHESIS);

                if (i < (cnt - 1))
                {
                    WriteNewLine(Fragments.COMMA);
                }
                else
                {
                    WriteNewLine(Fragments.STATEMENT_END, Indentation.Outer);
                }
            }

            WriteNewLine(builder.GenerateSelectSql());

            Write(builder.GenerateDropSql());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private void FormatInsertWithUserDefinedTableType(DbInsertExpression insert)
        {
            DbUserDefinedTableBuilder builder = new DbUserDefinedTableBuilder(insert.Table.Model, insert.Values);

            string 
                input_parameter_name = "input",
                output_parameter_name = "@output";

            WriteNewLine($"{Fragments.DECLARE} {output_parameter_name} {insert.Table.Model.DefinedTableType.QualifiedName};");
            WriteNewLine($"{Fragments.INSERT_INTO} {insert.Table.QualifiedName}");
            WriteNewLine($"{Fragments.OUTPUT_INSERTED_INTO} {output_parameter_name}");
            WriteNewLine(builder.GenerateSelectSql(
                $"@{input_parameter_name}", 
                builder.GetColumnInformation()
                    .Where(column =>
                        column.IsPrimaryKey == false)));
            WriteNewLine(builder.GenerateSelectSql(output_parameter_name));

            insert.DbParameters.Add(builder.CreateParameter(input_parameter_name, builder.GenerateTable()));
        }
    }
}
