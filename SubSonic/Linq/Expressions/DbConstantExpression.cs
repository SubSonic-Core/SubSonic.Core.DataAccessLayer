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

        protected internal DbConstantExpression(object value, Type type)
            : base((DbExpressionType)ExpressionType.Constant, type)
        {
            _constant = Constant(value);
        }

        protected internal DbConstantExpression(object value, Type type, TableAlias alias)
            : this(value, type)
        {
            _alias = alias;
        }

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
        public static DbExpression DbConstant(object value, Type type, TableAlias table)
        {
            return new DbConstantExpression(value, type, table);
        }

        public static DbExpression DbConstant(object value, Type type)
        {
            return new DbConstantExpression(value, type);
        }
    }
}
