using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbRowNumberExpression
        : DbExpression
    {
        protected internal DbRowNumberExpression(IEnumerable<DbOrderByDeclaration> orderBy)
            : base(DbExpressionType.RowCount, typeof(int))
        {
            OrderBy = orderBy as ReadOnlyCollection<DbOrderByDeclaration>;

            if (OrderBy == null && orderBy != null)
            {
                OrderBy = new List<DbOrderByDeclaration>(orderBy).AsReadOnly();
            }
        }
        public ReadOnlyCollection<DbOrderByDeclaration> OrderBy { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitRowNumber(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbRowNumber(IEnumerable<DbOrderByDeclaration> orderBy)
        {
            return new DbRowNumberExpression(orderBy);
        }
    }
}
