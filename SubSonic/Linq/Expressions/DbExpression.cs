using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure;
    using Structure;
    using SubSonic.Infrastructure.Builders;
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

        public static DbExpression Where(DbTableExpression table, Type type, LambdaExpression predicate)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return DbWherePredicateBuilder.GetWherePredicate(table, type, predicate);
        }
    }
}
