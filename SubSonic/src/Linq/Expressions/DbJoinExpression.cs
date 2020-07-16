using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;
    using SubSonic;
    using SubSonic.Builders;
    using SubSonic.Schema;
    using System;

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
        public static DbExpression DbJoin(JoinType joinType, DbExpression left, DbExpression right, Expression condition)
        {
            return new DbJoinExpression(joinType, left, right, condition);
        }

        public static DbExpression DbJoin(JoinType joinType, DbExpression left, DbExpression right)
        {
            Expression conditional = DbJoinConditional(left, right);

            return new DbJoinExpression(joinType, left, right, conditional);
        }

        public static Expression DbJoinConditional(DbExpression left, DbExpression right)
        {
            if (left is DbTableExpression _left)
            {
                if (right is DbTableExpression _right)
                {
                    Expression conditional = null;

                    if (_left.Model == _right.Model)
                    {   // self join
                        foreach (DbColumnDeclaration left_column in _left.Columns.Where(column => column.Property.IsPrimaryKey))
                        {
                            DbColumnDeclaration right_column = _right.Columns.Single(column => column.PropertyName == left_column.PropertyName);

                            if (conditional is null)
                            {
                                conditional = DbWherePredicateBuilder.GetComparisonExpression(right_column.Expression, left_column.Expression, DbComparisonOperator.Equal);
                            }
                            else
                            {
                                conditional = DbWherePredicateBuilder.GetBodyExpression(conditional, DbWherePredicateBuilder.GetComparisonExpression(right_column.Expression, left_column.Expression, DbComparisonOperator.Equal), DbGroupOperator.And);
                            }
                        }
                    }
                    else
                    {
                        IDbEntityProperty navigation = _right.Model.GetNavigationPropertyFor(_left.Model);

                        string[] 
                            primary = _left.Model.GetPrimaryKey().ToArray(),
                            foreign = navigation.ForeignKeys.ToArray();

                        System.Diagnostics.Debug.Assert(primary.Length == foreign.Length);

                        for(int i = 0, n = primary.Length; i < n; i++)
                        {
                            DbColumnDeclaration 
                                left_column = _left.Columns.Single(x => x.PropertyName.Equals(primary[i], StringComparison.Ordinal)),
                                right_column = _right.Columns.Single(x => x.PropertyName.Equals(foreign[i], StringComparison.Ordinal));

                            if (conditional is null)
                            {
                                conditional = DbWherePredicateBuilder.GetComparisonExpression(right_column.Expression, left_column.Expression, DbComparisonOperator.Equal);
                            }
                            else
                            {
                                conditional = DbWherePredicateBuilder.GetBodyExpression(conditional, DbWherePredicateBuilder.GetComparisonExpression(right_column.Expression, left_column.Expression, DbComparisonOperator.Equal), DbGroupOperator.And);
                            }
                        }
                    }

                    return conditional;
                }
            }

            throw new NotSupportedException();
        }
    }
}
