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

        protected TSqlFormatter WriteLine(Indentation style)
        {
            writer.WriteLine();

            Indent(style);

            for (int i = 0, n = (depth * IndentationWidth); i < n; i++)
            {
                Write(sqlContext.SqlFragment.SPACE);
            }

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
                        WriteLine(Indentation.Same);
                    }
                }
            }
            else
            {
                writer.Write(text);
            }

            return this;
        }
    }
}
