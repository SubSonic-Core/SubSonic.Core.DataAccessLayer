using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    using Infrastructure;

    public abstract partial class DbExpressionVisitor
    {
        protected virtual Expression VisitWhere(DbWhereExpression where)
        {
            if (where.IsNotNull())
            {
                return Visit(where.Expression);
            }
            return where;
        }
    }
}
