using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure;
    using Infrastructure.Builders;
    using Structure;

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

        public Expression Expression { get; }

        public IReadOnlyCollection<SubSonicParameter> Parameters { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitWhere(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression Where(DbTableExpression table, Type type, LambdaExpression predicate, DbExpressionType whereType = DbExpressionType.Where)
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

            if (whereType.NotIn(DbExpressionType.Where, DbExpressionType.Exists, DbExpressionType.NotExists))
            {
                throw new ArgumentException("", nameof(whereType));
            }

            return DbWherePredicateBuilder.GetWherePredicate(table, type, predicate, whereType);
        }
    }
}
