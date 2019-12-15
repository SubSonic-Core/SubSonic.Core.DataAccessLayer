using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure;

    public class DbWhereExpression
        : DbExpression
    {
        public DbWhereExpression(Type type, Expression predicate, IReadOnlyCollection<SubSonicParameter> parameters = null) 
            : base(DbExpressionType.Where, type)
        {
            Expression = predicate ?? throw new ArgumentNullException(nameof(predicate));
            Parameters = parameters ?? new ReadOnlyCollection<SubSonicParameter>(Array.Empty<SubSonicParameter>());
        }

        public override Expression Expression { get; }

        public IReadOnlyCollection<SubSonicParameter> Parameters { get; }
    }
}
