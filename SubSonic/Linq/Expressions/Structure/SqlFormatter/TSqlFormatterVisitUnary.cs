using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected override Expression VisitUnary(UnaryExpression unary)
        {
            if (unary.IsNotNull())
            {
                string op = GetOperator(unary);

                switch (unary.NodeType)
                {
                    case ExpressionType.Not:
                        if (unary.Operand.Type.IsBoolean())
                        {
                            Write(op);
                            Write(sqlContext.SqlFragment.SPACE);
                            this.VisitPredicate(unary.Operand);
                        }
                        else
                        {
                            Write(op);
                            this.VisitValue(unary.Operand);
                        }
                        break;
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        Write(op);
                        this.VisitValue(unary.Operand);
                        break;
                    case ExpressionType.UnaryPlus:
                        this.VisitValue(unary.Operand);
                        break;
                    case ExpressionType.Convert:
                        // ignore conversions for now
                        Visit(unary.Operand);
                        break;
                    default:
                        ThrowUnaryNotSupported(unary);
                        break;
                }
            }
            return unary;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected void ThrowUnaryNotSupported(UnaryExpression unary) => throw new NotSupportedException(SubSonicErrorMessages.UnSupportedUnaryOperator.Format($"{unary.NodeType}"));
    }
}
