﻿using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbOuterJoinedExpression : DbExpression
    {
        public DbOuterJoinedExpression(Expression test, Expression expression)
            : base(DbExpressionType.OuterJoined, expression.Type)
        {
            Test = test ?? throw new ArgumentNullException(nameof(test));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Expression Test { get; }

        public Expression Expression { get; }
    }
}