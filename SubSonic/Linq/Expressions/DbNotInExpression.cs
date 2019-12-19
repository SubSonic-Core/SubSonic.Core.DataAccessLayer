using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbNotInExpression
        : DbInExpression
    {
        public DbNotInExpression(Expression left, Expression inside)
            : base(DbExpressionType.NotIn, left, inside) { }
    }
}
