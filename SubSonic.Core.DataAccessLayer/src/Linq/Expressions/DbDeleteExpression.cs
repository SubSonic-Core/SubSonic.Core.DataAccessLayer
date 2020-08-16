using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using SubSonic.Core.DAL.src;
    using Structure;

    public class DbDeleteExpression
        : DbConstantExpression
    {
        protected internal DbDeleteExpression(
            object collection,
            DbExpression from,
            Expression where)
            : base(
                  collection, 
                  from.IsNullThrowArgumentNull(nameof(from)).Type)
        {
            if (!(from is DbTableExpression))
            {
                throw new InvalidOperationException(SubSonicErrorMessages.DbExpressionMustBeOfType.Format(nameof(DbTableExpression)));
            }

            From = (DbTableExpression)from;
            Where = where;
        }

        public override ExpressionType NodeType => (ExpressionType)DbExpressionType.Delete;

        public DbTableExpression From { get; }

        public Expression Where { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitDelete(this);
            }

            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return SubSonicContext.GenerateSqlFor(this);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbDelete(object collection, DbExpression from)
        {
            return new DbDeleteExpression(collection, from, null);
        }

        public static DbExpression DbDelete(object collection, DbExpression from, Expression where)
        { 
            if (where is null)
            {
                throw Error.ArgumentNull(nameof(where));
            }

            return new DbDeleteExpression(collection, from, where);
        }
    }
}
