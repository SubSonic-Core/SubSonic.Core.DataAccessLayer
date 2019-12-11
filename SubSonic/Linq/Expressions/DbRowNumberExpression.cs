using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SubSonic.Linq.Expressions
{
    public class DbRowNumberExpression
        : DbExpression
    {
        public DbRowNumberExpression(IEnumerable<DbOrderByDeclaration> orderBy)
            : base(DbExpressionType.RowCount, typeof(int))
        {
            OrderBy = orderBy as ReadOnlyCollection<DbOrderByDeclaration>;

            if (OrderBy == null && orderBy != null)
            {
                OrderBy = new List<DbOrderByDeclaration>(orderBy).AsReadOnly();
            }
        }
        public ReadOnlyCollection<DbOrderByDeclaration> OrderBy { get; }
    }
}
