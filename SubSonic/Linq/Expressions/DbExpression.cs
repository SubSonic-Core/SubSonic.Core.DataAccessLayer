using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure;
    using Structure;
    using System.Collections.Generic;

    public abstract class DbExpression : Expression
    {
        protected DbExpression(DbExpressionType eType, Type type)
            : base()
        {
            NodeType = (ExpressionType)eType;
            Type = type;
        }

        public virtual Expression Expression { get; }

        public override ExpressionType NodeType { get; }

        public override Type Type { get; }

        public override string ToString()
        {
            return DbExpressionWriter.WriteToString(this);
        }

        public static DbWhereExpression Where(Type type, Expression predicate, IReadOnlyCollection<SubSonicParameter> parameters = null)
        {
            return new DbWhereExpression(type, predicate, parameters);
        }
    }
}
