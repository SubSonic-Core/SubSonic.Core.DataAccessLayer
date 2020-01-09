// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
//Original code created by Matt Warren: http://iqtoolkit.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=19725

// refactored by Kenneth Carter (c) 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected internal override Expression VisitFunction(DbFunctionExpression dbFunction)
        {
            if (dbFunction is DbIsNullFunctionExpression fn)
            {
                Write($"ISNULL{Fragments.LEFT_PARENTHESIS}");



                for(int i = 0, cnt = fn.Arguments.Count(); i < cnt; i++)
                {
                    Visit(fn.Arguments.ElementAt(i));
                    if (i < (cnt - 1))
                    {
                        Write($"{Fragments.COMMA} ");
                    }
                }
                Write(Fragments.RIGHT_PARENTHESIS);

                return fn;
            }
            return base.VisitFunction(dbFunction);
        }
    }
}
