using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbDeleteExpression
        : DbConstantExpression
    {
        protected internal DbDeleteExpression(
            object collection,
            DbExpression from,
            Expression where)
            : base(collection, null)
        {
            if (from.IsNull())
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (!(from is DbTableExpression))
            {
                throw new InvalidOperationException(SubSonicErrorMessages.DbExpressionMustBeOfType.Format(nameof(DbTableExpression)));
            }

            From = (DbTableExpression)from;
            Where = where ?? throw new ArgumentNullException(nameof(where));
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
            return DbContext.GenerateSqlFor(this);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbDelete(object collection, DbExpression from, Expression where)
        {
            return new DbDeleteExpression(collection, from, where);
        }
    }
}
