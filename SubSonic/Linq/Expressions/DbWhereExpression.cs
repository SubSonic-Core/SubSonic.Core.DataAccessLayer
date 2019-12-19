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
        public DbWhereExpression(DbExpressionType eType, Type type, LambdaExpression lambda, Expression predicate, IReadOnlyCollection<SubSonicParameter> parameters = null) 
            : base(eType, eType == DbExpressionType.Where ? type : typeof(bool))
        {
            LambdaPredicate = lambda ?? throw new ArgumentNullException(nameof(lambda));
            Expression = predicate ?? throw new ArgumentNullException(nameof(predicate));
            Parameters = parameters ?? new ReadOnlyCollection<SubSonicParameter>(Array.Empty<SubSonicParameter>());
        }

        public LambdaExpression LambdaPredicate { get; }

        public override Expression Expression { get; }

        public IReadOnlyCollection<SubSonicParameter> Parameters { get; }
    }
}
