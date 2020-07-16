using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbClientJoinExpression : DbExpression
    {
        protected internal DbClientJoinExpression(DbProjectionExpression projection, IEnumerable<Expression> outerKey, IEnumerable<Expression> innerKey)
            : base(DbExpressionType.ClientJoin, projection.IsNullThrowArgumentNull(nameof(projection)).Type)
        {
            OuterKey = outerKey as ReadOnlyCollection<Expression>;
            if (OuterKey == null)
            {
                OuterKey = new List<Expression>(outerKey).AsReadOnly();
            }
            InnerKey = innerKey as ReadOnlyCollection<Expression>;
            if (InnerKey == null)
            {
                InnerKey = new List<Expression>(innerKey).AsReadOnly();
            }
            Projection = projection;
        }

        public ReadOnlyCollection<Expression> OuterKey { get; }

        public ReadOnlyCollection<Expression> InnerKey { get; }

        public DbProjectionExpression Projection { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitClientJoin(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbClientJoin(DbProjectionExpression projection, IEnumerable<Expression> outerKey, IEnumerable<Expression> innerKey)
        {
            return new DbClientJoinExpression(projection, outerKey, innerKey);
        }
    }
}
