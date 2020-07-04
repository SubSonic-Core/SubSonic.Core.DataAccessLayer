using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public abstract class DbFunctionExpression
        : DbExpression
    {
        protected DbFunctionExpression(DbExpressionType eType, params Expression[] arguments) 
            : base(eType, null)
        {
            if(eType.NotIn(SupportedFunctions.ToArray()))
            {
                throw new ArgumentException("", nameof(eType));
            }

            if (arguments.Length == 0)
            {
                throw new ArgumentException("", nameof(arguments));
            }

            Type = arguments[0].Type.GetUnderlyingType();
            Arguments = arguments;
        }

        public override Type Type { get; }

        public virtual IEnumerable<Expression> Arguments { get; }

        protected virtual IEnumerable<DbExpressionType> SupportedFunctions => new DbExpressionType[] { DbExpressionType.IsNull };

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitFunction(this);
            }
            return base.Accept(visitor);
        }
    }
}
