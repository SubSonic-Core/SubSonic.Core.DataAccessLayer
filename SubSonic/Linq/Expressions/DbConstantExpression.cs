using SubSonic.Linq.Expressions.Structure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    using Alias;

    public class DbConstantExpression
        : DbExpression
    {
        private readonly ConstantExpression _constant;
        private readonly TableAlias _alias;

        protected internal DbConstantExpression(object value, TableAlias alias)
            : base((DbExpressionType)ExpressionType.Constant, value.IsNullThrowArgumentNull(nameof(value)).GetType())
        {
            _constant = Constant(value);
            _alias = alias;
        }

        internal static DbConstantExpression Build(object value, TableAlias alias)
        {
            return new DbConstantExpression(value, alias);
        }

        public sealed override Type Type => _constant.Type;

        public object QueryObject => _constant.Value;

        public TableAlias Alias => _alias;

        public sealed override bool CanReduce => true;

        public override ExpressionType NodeType => _constant.NodeType;

        public sealed override Expression Reduce()
        {
            return _constant;
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbConstant(object value, TableAlias table)
        {
            return new DbConstantExpression(value, table);
        }
    }
}
