using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    public class DbIsNullFunctionExpression
        : DbFunctionExpression
    {
        protected internal DbIsNullFunctionExpression(params Expression[] arguments) 
            : base(DbExpressionType.IsNull, arguments)
        {

        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbIsNull(params Expression[] arguments)
        {
            return new DbIsNullFunctionExpression(arguments);
        }
    }
}
