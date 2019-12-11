using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// A custom expression node representing a SQL join clause
    /// </summary>
    public class DbJoinExpression : DbExpression
    {
        JoinType joinType;
        Expression left;
        Expression right;
        Expression condition;

        public DbJoinExpression(JoinType joinType, Expression left, Expression right, Expression condition)
            : base(DbExpressionType.Join, typeof(void))
        {
            this.joinType = joinType;
            this.left = left;
            this.right = right;
            this.condition = condition;
        }
        public JoinType Join
        {
            get { return joinType; }
        }
        public Expression Left
        {
            get { return left; }
        }
        public Expression Right
        {
            get { return right; }
        }
        public new Expression Condition
        {
            get { return condition; }
        }
    }
}
