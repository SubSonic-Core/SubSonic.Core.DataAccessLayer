// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected internal override Expression VisitWhere(DbWhereExpression where)
        {
            if (where.IsNotNull())
            {
                WriteNewLine();
                Write($"{Fragments.WHERE} ");

                if (((DbExpressionType)where.NodeType).In(DbExpressionType.Exists, DbExpressionType.NotExists))
                {
                    if(((DbExpressionType)where.NodeType) == DbExpressionType.NotExists)
                    {
                        Write($"{Fragments.NOT} ");
                    }
                    Write($"{Fragments.EXISTS} {Fragments.LEFT_PARENTHESIS}");
                    WriteNewLine(Indentation.Inner);
                }

                if (((DbExpressionType)where.NodeType) == DbExpressionType.Where && !IsPredicate(where.Expression))
                {
                    Write(Fragments.LEFT_PARENTHESIS);
                    Visit(where.Expression);
                    Write($"{Fragments.RIGHT_PARENTHESIS} <> 0");
                }
                else
                {
                    Visit(where.Expression);
                }

                if (((DbExpressionType)where.NodeType).In(DbExpressionType.Exists, DbExpressionType.NotExists))
                {
                    Write($"{Fragments.RIGHT_PARENTHESIS}");
                    Indent(Indentation.Outer);
                }
            }
            return where;
        }
    }
}
