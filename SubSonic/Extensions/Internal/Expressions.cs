using System.Linq.Expressions;

namespace SubSonic
{
    using Linq.Expressions;
    using System;
    using System.Reflection;

    internal static partial class InternalExtensions
    {
        public static Func<TSource, bool> CombinePredicates<TSource>(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2) =>
            (x) => predicate1(x) && predicate2(x);

        public static Expression ReBuild(this Expression expression, ParameterExpression parameter)
        {
            if (expression is BinaryExpression binary)
            {
                return Expression.MakeBinary(binary.NodeType, binary.Left.ReBuild(parameter), binary.Right.ReBuild(parameter));
            }
            else if (expression is MemberExpression access)
            {
                if (access.Member is PropertyInfo pi)
                {
                    return Expression.Property(parameter, pi);
                }
                else if (access.Member is FieldInfo fi)
                {
                    return Expression.Field(parameter, fi);
                }
            }
            else if (expression is ConstantExpression constant)
            {
                return constant;
            }

            return expression;
        }
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
