using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure.Builders;
    using Structure;

    public class DbWhereExpression
        : DbExpression
    {
        public DbWhereExpression(DbExpressionType eType, Type type, LambdaExpression lambda, Expression predicate, bool canReadFromCache, IReadOnlyCollection<DbParameter> parameters = null) 
            : base(eType, eType == DbExpressionType.Where ? type : typeof(bool))
        {
            LambdaPredicate = lambda ?? throw new ArgumentNullException(nameof(lambda));
            Expression = predicate ?? throw new ArgumentNullException(nameof(predicate));
            CanReadFromCache = canReadFromCache;
            Parameters = parameters ?? new ReadOnlyCollection<DbParameter>(Array.Empty<DbParameter>());
        }

        public bool CanReadFromCache { get; }

        public LambdaExpression LambdaPredicate { get; }

        public Expression Expression { get; }

        public IReadOnlyCollection<DbParameter> Parameters { get; }

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
        public static DbExpression DbWhere(DbTableExpression table, Type type, LambdaExpression predicate, DbExpressionType whereType = DbExpressionType.Where)
        {
            if (table is null)
            {
                throw Error.ArgumentNull(nameof(table));
            }

            if (type is null)
            {
                throw Error.ArgumentNull(nameof(type));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }

            if (whereType.NotIn(DbExpressionType.Where, DbExpressionType.Exists, DbExpressionType.NotExists))
            {
                throw Error.Argument("", nameof(whereType));
            }

            return DbWherePredicateBuilder.GetWherePredicate(type, predicate, whereType, table);
        }
    }
}
