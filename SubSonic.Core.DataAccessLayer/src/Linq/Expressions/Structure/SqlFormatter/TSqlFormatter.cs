// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using SubSonic.Core.DAL.src;
    using Alias;
    using SqlGenerator;
    using Factory;


    public partial class TSqlFormatter
        : DbExpressionVisitor
    {
        [ThreadStatic]
        private static Stack<TSqlFormatter> __formatter_instances;
        private int depth;
        private readonly TextWriter writer;
        private readonly ISqlContext context;
        private readonly TableAliasCollection aliases = new TableAliasCollection();
        private readonly SubSonicDbProvider provider;


        public static string Format(ISqlContext context, Expression expression)
        {
            StringBuilder builder = new StringBuilder();

            using (TextWriter writer = new StringWriter(builder))
            using (TSqlFormatter sqlFormatter = new TSqlFormatter(writer, context))
            {
                sqlFormatter.Visit(expression);

                return builder.ToString();
            }
        }

        protected TSqlFormatter(TextWriter writer, ISqlContext context)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.provider = context.Provider ?? throw new InvalidOperationException();

            if (__formatter_instances is null)
            {
                __formatter_instances = new Stack<TSqlFormatter>();
            }

            __formatter_instances.Push(this);
        }

        protected static int IndentationWidth => __formatter_instances.Count;

        protected bool IsNested { get; set; }

        protected ISqlFragment Fragments => context.Fragments;
        
        protected TSqlFormatter Indent(Indentation style)
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
            return this;
        }

        protected TSqlFormatter WriteNewLine(Indentation style)
        {
            Indent(style);

            return WriteNewLine();
        }

        protected TSqlFormatter WriteNewLine(string text, Indentation style = Indentation.Same)
        {
            if (text.IsNotNullOrEmpty())
            {
                Write(text);
            }

            Indent(style);

            WriteNewLine();

            return this;
        }

        protected TSqlFormatter WriteNewLine()
        {
            writer.WriteLine();

            for (int i = 0, n = (depth * IndentationWidth); i < n; i++)
            {
                Write(special[1]);
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

#if NETSTANDARD2_0
            if (text.IndexOf(special[0]) >= 0)
#elif NETSTANDARD2_1
            if (text.IndexOf(special[0], StringComparison.CurrentCulture) >= 0)
#endif
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
#if NETSTANDARD2_0
                return value.Replace(context.Fragments.QOUTE, $"{context.Fragments.QOUTE}{context.Fragments.QOUTE}");
#elif NETSTANDARD2_1
                return value.Replace(context.Fragments.QOUTE, $"{context.Fragments.QOUTE}{context.Fragments.QOUTE}", StringComparison.CurrentCulture);
#endif
            }
            return string.Empty;
        }

        protected virtual void WriteValue(object value)
        {
            if (value == null)
            {
                Write(context.Fragments.NULL);
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        Write(((bool)value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        Write(context.Fragments.QOUTE);
                        Write(EscapeString((string)value));
                        Write(context.Fragments.QOUTE);
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
                            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.UnSupportedConstant, value.GetType().GetTypeName()));
                        }
                        break;
                    default:
                        Write(value);
                        break;
                }
            }
        }

        protected string GetAliasName(TableAlias alias)
        {
            if (alias is null)
            {
                throw new ArgumentNullException(nameof(alias));
            }

            return aliases.GetAliasName(alias);
        }

        private string GetAggregateName(AggregateType aggregateType)
        {
            switch (aggregateType)
            {
                case AggregateType.Count: return context.Fragments.COUNT;
                case AggregateType.LongCount: return context.Fragments.COUNTBIG;
                case AggregateType.Min: return context.Fragments.MIN;
                case AggregateType.Max: return context.Fragments.MAX;
                case AggregateType.Sum: return context.Fragments.SUM;
                case AggregateType.Average: return context.Fragments.AVG;
                default: throw new Exception(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.UnknownAggregate, aggregateType));
            }
        }

        private bool RequiresAsteriskWhenNoArgument(AggregateType aggregateType)
        {
            return aggregateType == AggregateType.Count || aggregateType == AggregateType.LongCount;
        }
    }
}
