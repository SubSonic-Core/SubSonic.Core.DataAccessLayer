using System;
using System.IO;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions.Structure
{
    using Microsoft;

    public partial class DbExpressionWriter
        : ExpressionWriter
    {
        public static void Write(TextWriter writer, Expression expression)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            new DbExpressionWriter(writer).Visit(expression);
        }

        public static string WriteToString(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (StringWriter writer = new StringWriter())
            {
                Write(writer, expression);

                return writer.ToString();
            }
        }

        protected DbExpressionWriter(TextWriter writer)
            : base(writer)
        {
        }
    }
}
