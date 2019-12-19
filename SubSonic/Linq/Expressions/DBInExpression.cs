using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbInExpression
        : DbSubQueryExpression
    {
        public DbInExpression(Expression left, Expression inside)
            : this(DbExpressionType.In)
        {
            Left = left ?? throw new System.ArgumentNullException(nameof(left));
            Inside = inside ?? throw new System.ArgumentNullException(nameof(inside));
        }

        protected DbInExpression(DbExpressionType eType, Expression left, Expression inside)
            : this(eType)
        {
            Left = left ?? throw new System.ArgumentNullException(nameof(left));
            Inside = inside ?? throw new System.ArgumentNullException(nameof(inside));
        }

        private DbInExpression(DbExpressionType eType)
            : base(eType, typeof(bool), null) { }

        public virtual Expression Left { get; }
        public virtual Expression Inside { get; }
    }
}
