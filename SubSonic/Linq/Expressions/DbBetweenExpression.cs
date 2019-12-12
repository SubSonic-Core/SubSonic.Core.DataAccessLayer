﻿using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbBetweenExpression : DbExpression
    {
        public DbBetweenExpression(Expression expression, Expression lower, Expression upper)
            : this(DbExpressionType.Between, expression.IsNullThrowArgumentNull(nameof(expression)).Type)
        {
            Expression = expression;
            Lower = lower ?? throw new ArgumentNullException(nameof(lower));
            Upper = upper ?? throw new ArgumentNullException(nameof(upper));
        }

        protected DbBetweenExpression(DbExpressionType eType, Type type)
            : base(eType, type) { }

        public override Expression Expression { get; }
        public virtual Expression Lower { get; }
        public virtual Expression Upper { get; }
    }
}