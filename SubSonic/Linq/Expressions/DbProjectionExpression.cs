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
        public DbProjectionExpression(DbSelectExpression source, Expression projector)
            : this(source, projector, null)
        {
        }
        public DbProjectionExpression(DbSelectExpression source, Expression projector, LambdaExpression aggregator)
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
            get { return TSqlFormatter.Format(Source); }
        }
    }
}
