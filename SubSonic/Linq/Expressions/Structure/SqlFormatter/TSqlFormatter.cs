// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Alias;
    using Infrastructure.SqlGenerator;
    using System.Globalization;

    public partial class TSqlFormatter
        : DbExpressionVisitor
    {
        private static readonly char[] splitters = new char[] { '\n', '\r' };
        private static readonly char[] special = new char[] { '\n', '\n', '\\' };
        private int depth = 0;
        private readonly TextWriter writer;
        private readonly ISqlContext sqlContext;
        private readonly Dictionary<Table, string> aliases;

        

        public static string Format(Expression expression, ISqlContext sqlContext = null)
        {
            StringBuilder builder = new StringBuilder();

            using (TextWriter writer = new StringWriter(builder))
            {
                TSqlFormatter sqlFormatter = new TSqlFormatter(writer, sqlContext);

                sqlFormatter.Visit(expression);

                return builder.ToString();
            }
        }

        protected TSqlFormatter(TextWriter writer, ISqlContext sqlContext)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            this.aliases = new Dictionary<Table, string>();
        }

        protected int IndentationWidth { get; set; } = 2;

        protected bool IsNested { get; set; } = false;

        protected void Indent(Indentation style)
        {
            if (style == Indentation.Inner)
            {
                this.depth++;
            }
            else if (style == Indentation.Outer)
            {
                this.depth--;
                System.Diagnostics.Debug.Assert(this.depth >= 0);
            }
        }

        protected TSqlFormatter WriteNewLine(Indentation style)
        {
            writer.WriteLine();

            Indent(style);

            for (int i = 0, n = (depth * IndentationWidth); i < n; i++)
            {
                Write(sqlContext.SqlFragment.SPACE);
            }

            return this;
        }

        protected TSqlFormatter WriteFormat(string text, params object[] args)
        {
            Write(text.Format(args));

            return this;
        }

        protected TSqlFormatter Write(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.IndexOf('\n', StringComparison.CurrentCulture) >= 0)
            {
                string[] lines = text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0, n = lines.Length; i < n; i++)
                {
                    Write(lines[i]);
                    if (i < n - 1)
                    {
                        WriteNewLine(Indentation.Same);
                    }
                }
            }
            else
            {
                writer.Write(text);
            }

            return this;
        }

        protected TSqlFormatter Write<TType>(TType value)
        {
            Write(Convert.ToString(value, CultureInfo.CurrentCulture));

            return this;
        }

        protected virtual string EscapeString(string value)
        {
            if (value.IsNotNullOrEmpty())
            {
                return value.Replace("'", "''", StringComparison.CurrentCulture);
            }
            return string.Empty;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        protected virtual void WriteValue(object value)
        {
            if (value == null)
            {
                Write("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        Write(((bool)value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        Write("'");
                        Write(EscapeString((string)value));
                        Write("'");
                        break;
                    case TypeCode.Decimal:
                        Write(((Decimal)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.Double:
                        Write(((Double)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.Int16:
                        Write(((Int16)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.Int32:
                        Write(((Int32)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.Int64:
                        Write(((Int64)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.UInt16:
                        Write(((UInt16)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.UInt32:
                        Write(((UInt32)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.UInt64:
                        Write(((UInt64)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.DateTime:
                        Write(((DateTime)value).ToString(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.Object:
                        if (value.GetType().IsEnum)
                        {
                            Write(Convert.ChangeType(value, typeof(int), CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            throw new NotSupportedException(SubSonicErrorMessages.UnSupportedConstant.Format(value.GetType().GetTypeName()));
                        }
                        break;
                    default:
                        Write(value);
                        break;
                }
            }
        }

        protected string GetAliasName(Table alias)
        {
            string name;
            if (!aliases.TryGetValue(alias, out name))
            {
                name = $"T:{aliases.Count}";
                aliases.Add(alias, name);
            }
            return name;
        }

        private string GetAggregateName(AggregateType aggregateType)
        {
            switch (aggregateType)
            {
                case AggregateType.Count: return "COUNT";
                case AggregateType.Min: return "MIN";
                case AggregateType.Max: return "MAX";
                case AggregateType.Sum: return "SUM";
                case AggregateType.Average: return "AVG";
                default: throw new Exception(SubSonicErrorMessages.UnknownAggregate.Format(aggregateType));
            }
        }

        private bool RequiresAsteriskWhenNoArgument(AggregateType aggregateType)
        {
            return aggregateType == AggregateType.Count;
        }
    }
}
