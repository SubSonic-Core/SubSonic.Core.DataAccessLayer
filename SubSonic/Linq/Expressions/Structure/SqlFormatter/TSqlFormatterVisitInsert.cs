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
                DbUserDefinedTableBuilder builder = new DbUserDefinedTableBuilder(insert.Into.Model,
                insert.Values
                .Select(entity =>
                {
                    if (entity is ConstantExpression constant)
                    {
                        return constant.Value;
                    }

                    return null;
                })
                .Where(value => value.IsNotNull()));

                string @output = "@output";

                WriteNewLine(builder.GenerateSql(true, @output));
                WriteNewLine($"{Fragments.INSERT_INTO} {insert.Into.QualifiedName}");
                WriteNewLine($"{Fragments.OUTPUT_INSERTED_INTO} {@output}");

                IEnumerable<DbColumnDeclaration> columns = insert.Into.Columns.Where(x =>
                    !x.Property.IsReadOnly &&
                    x.Property.EntityPropertyType == DbEntityPropertyType.Value);

                if (insert.Into.Model.DefinedTableTypeExists)
                {
                    FormatInsertWithUserDefinedTableType(insert, builder);
                }
                else
                {
                    FormatInsertUsingParameters(insert, columns);
                }

                Write(builder.GenerateSelectSql(@output));
            }

            return insert;
        }

        private void FormatInsertUsingParameters(DbInsertExpression insert, IEnumerable<DbColumnDeclaration> columns)
        {
            WriteNewLine(Fragments.VALUES);

            WriteNewLine(Indentation.Inner);

            for (int iValue = 0, vCount = insert.Values.Count(); iValue < vCount; iValue++)
            {
                Write(Fragments.LEFT_PARENTHESIS);

                if (insert.Values.ElementAt(iValue) is ConstantExpression entity)
                {
                    for (int iColumn = 0, cCount = columns.Count(); iColumn < cCount; iColumn++)
                    {
                        DbColumnDeclaration column = columns.ElementAt(iColumn);

                        string parameterName = $"@{column.PropertyName}";

                        Write(parameterName);

                        if (insert.DbParameters.Count(x =>
                                x.ParameterName.Equals(parameterName, StringComparison.CurrentCulture)) == 0)
                        {
                            object value = insert.Type.BaseType
                            .GetProperty(column.PropertyName)
                            .GetValue(entity.Value);

                            insert.DbParameters.Add(provider.CreateParameter(parameterName, value ?? DBNull.Value, column.Property));
                        }

                        if (iColumn < (cCount - 1))
                        {
                            Write($"{Fragments.COMMA} ");
                        }
                    }
                }

                Write(Fragments.RIGHT_PARENTHESIS);

                if (iValue < (vCount - 1))
                {
                    WriteNewLine(Fragments.COMMA);
                }
                else
                {
                    WriteNewLine(Fragments.STATEMENT_END, Indentation.Outer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private void FormatInsertWithUserDefinedTableType(DbInsertExpression insert, DbUserDefinedTableBuilder builder)
        {
            string 
                input_parameter_name = "input";

            WriteNewLine(builder.GenerateSelectSql(
                $"@{input_parameter_name}", 
                builder.GetColumnInformation()
                    .Where(column =>
                        column.IsIdentity == false && column.IsComputed == false)));

            insert.DbParameters.Add(builder.CreateParameter(input_parameter_name, builder.GenerateTable()));
        }
    }
}
