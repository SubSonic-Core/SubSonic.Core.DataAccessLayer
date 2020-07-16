using SubSonic;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;
    /// <summary>
    /// A custom expression representing the construction of one or more result objects from a 
    /// SQL select expression
    /// </summary>
    public class DbProjectionExpression : DbExpression
    {
        protected internal DbProjectionExpression(DbSelectExpression source, Expression projector, LambdaExpression aggregator)
            : base(DbExpressionType.Projection, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.IsNullThrowArgumentNull(nameof(projector)).Type))
        {
            Source = source;
            Projector = projector;
            Aggregator = aggregator;
        }
        public DbSelectExpression Source { get; }
        public Expression Projector { get; }
        public LambdaExpression Aggregator { get; }
        public bool IsSingleton
        {
            get { return Aggregator != null && Aggregator.Body.Type == Projector.Type; }
        }
        public override string ToString()
        {
            return DbExpressionWriter.WriteToString(this);
        }
        public string QueryText
        {
            get { return SubSonicContext.GenerateSqlFor(this); }
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitProjection(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbProjection(DbSelectExpression source, Expression projector)
        {
            return new DbProjectionExpression(source, projector, null);
        }

        public static DbExpression DbProjection(DbSelectExpression source, Expression projector, LambdaExpression aggregator)
        {
            return new DbProjectionExpression(source, projector, aggregator);
        }
    }
}