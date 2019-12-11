using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public class DbClientJoinExpression : DbExpression
    {
        public DbClientJoinExpression(DbProjectionExpression projection, IEnumerable<Expression> outerKey, IEnumerable<Expression> innerKey)
            : base(DbExpressionType.ClientJoin, projection.IsNullThrowMissingArgument(nameof(projection)).Type)
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
    }
}
