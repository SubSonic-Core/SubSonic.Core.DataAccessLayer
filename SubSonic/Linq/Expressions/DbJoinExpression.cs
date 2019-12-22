using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    /// <summary>
    /// A custom expression node representing a SQL join clause
    /// </summary>
    public class DbJoinExpression : DbExpression
    {
        protected internal DbJoinExpression(JoinType joinType, Expression left, Expression right, Expression condition)
            : base(DbExpressionType.Join, typeof(void))
        {
            Join = joinType;
            Left = left;
            Right = right;
            Condition = condition;
        }
        public JoinType Join { get; }
        public Expression Left { get; }
        public Expression Right { get; }
        public new Expression Condition { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitJoin(this);
            }
            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbJoin(JoinType joinType, Expression left, Expression right, Expression condition)
        {
            return new DbJoinExpression(joinType, left, right, condition);
        }
    }
}
