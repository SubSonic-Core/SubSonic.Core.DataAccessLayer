using System.Linq.Expressions;

namespace SubSonic
{
    using Linq.Expressions;
    using System;
    using System.Reflection;

    internal static partial class InternalExtensions
    {
        public static string GetPropertyName<TEntity, TColumn>(this Expression<Func<TEntity, TColumn>> lambda)
        {
            return ((LambdaExpression)lambda).GetProperty()?.Name;
        }

        public static PropertyInfo GetProperty(this LambdaExpression lambda)
        {
            return ((MemberExpression)lambda.Body).Member as PropertyInfo;
        }
        public static bool CanBeColumn(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubQuery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDbExpression(this ExpressionType et)
        {
            return ((int)et) >= 1000;
        }
    }
}
