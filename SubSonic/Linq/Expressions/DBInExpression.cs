using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;
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

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitIn(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbIn(Expression left, Expression inside)
        {
            return new DbInExpression(left, inside);
        }
    }
}
