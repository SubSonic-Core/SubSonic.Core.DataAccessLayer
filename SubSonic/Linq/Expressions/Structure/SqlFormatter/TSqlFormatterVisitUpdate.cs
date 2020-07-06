using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    using Infrastructure;

    public partial class TSqlFormatter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "this datatable is disposed after the result from the db is returned.")]
        protected internal override Expression VisitUpdate(DbUpdateExpression update)
        {
            if (update.IsNotNull())
            {
                DbUserDefinedTableBuilder builder = new DbUserDefinedTableBuilder(update.From.Model,
                update.Data
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
                WriteNewLine($"{Fragments.UPDATE} [{GetAliasName(update.From.Alias)}] {Fragments.SET}", Indentation.Inner);

                IEnumerable<DbColumnDeclaration> columns = update.From.Columns.Where(x =>
                    !x.Property.IsReadOnly &&
                    x.Property.EntityPropertyType == DbEntityPropertyType.Value);

                if (update.From.Model.DefinedTableTypeExists)
                {
                    FormatUpdateUsingDefinedTableType(update, columns);

                    if (update.From.Joins.ElementAt(0) is DbJoinExpression join)
                    {
                        if (join.Right is DbTableExpression table)
                        {
                            if (!update.DbParameters.Any(x =>
                                x.ParameterName.Equals($"@{table.QualifiedName}", StringComparison.CurrentCulture)))
                            {
                                update.DbParameters.Add(builder.CreateParameter(table.QualifiedName, builder.GenerateTable()));
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                else
                {
                    FormatUpdateUsingParameters(update, columns);
                }

                WriteNewLine(Indentation.Outer);
                WriteNewLine($"{Fragments.OUTPUT_INSERTED_INTO} {@output}");
                Write($"{Fragments.FROM} ");

                VisitSource(update.From);

                foreach (DbExpression expression in update.From.Joins)
                {
                    VisitSource(expression);
                }

                if (update.Where.IsNotNull())
                {
                    Visit(update.Where);
                }

                WriteNewLine(Fragments.STATEMENT_END);

                Write(builder.GenerateSelectSql(@output));
            }

            return update;
        }

        private void FormatUpdateUsingParameters(DbUpdateExpression update, IEnumerable<DbColumnDeclaration> columns)
        {
            foreach (ConstantExpression entity in update.Data)
            {
                for (int i = 0, n = columns.Count(); i < n; i++)
                {
                    DbColumnDeclaration column = columns.ElementAt(i);

                    string parameterName = $"@{column.PropertyName}";

                    Visit(column.Expression);

                    Write($" {Fragments.EQUAL_TO} {parameterName}");

                    if (!update.DbParameters.Any(x =>
                            x.ParameterName.Equals(parameterName, StringComparison.CurrentCulture)))
                    {
                        object value = update.Type.GenericTypeArguments[0]
                        .GetProperty(column.PropertyName)
                        .GetValue(entity.Value);

                        update.DbParameters.Add(provider.CreateParameter(parameterName, value ?? DBNull.Value, column.Property));
                    }

                    if (i < (n - 1))
                    {
                        WriteNewLine(Fragments.COMMA);
                    }
                }
            }
        }

        private void FormatUpdateUsingDefinedTableType(DbUpdateExpression update, IEnumerable<DbColumnDeclaration> columns)
        {
            if (update.From.Joins.ElementAt(0) is DbJoinExpression dbJoin)
            {
                if (dbJoin.Right is DbTableExpression tableType)
                {
                    IEnumerable<DbColumnDeclaration> tableTypeColumns = tableType.Columns.Where(x => !x.Property.IsReadOnly);

                    for (int i = 0, n = columns.Count(); i < n; i++)
                    {
                        DbColumnDeclaration
                            tableColumn = columns.ElementAt(i),
                            tableTypeColumn = tableTypeColumns.ElementAt(i);

                        Debug.Assert(tableColumn.PropertyName.Equals(tableTypeColumn.PropertyName, StringComparison.CurrentCulture));

                        Visit(tableColumn.Expression);

                        Write($" {Fragments.EQUAL_TO} ");

                        Visit(tableTypeColumn.Expression);

                        if (i < (n - 1))
                        {
                            WriteNewLine(Fragments.COMMA);
                        }
                    } 
                }
            }
        }
    }
}
