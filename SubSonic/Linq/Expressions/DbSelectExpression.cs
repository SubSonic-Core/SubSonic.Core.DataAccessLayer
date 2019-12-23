using SubSonic.Linq.Expressions.Alias;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure;
    using Structure;

    /// <summary>
    /// A custom expression node used to represent a SQL SELECT expression
    /// </summary>
    public class DbSelectExpression
        : DbConstantExpression
    {
        protected internal DbSelectExpression(
            object collection,
            DbTableExpression table)
            : this(collection, table, table.IsNullThrowArgumentNull(nameof(table)).Columns) { }

        protected internal DbSelectExpression(
            object collection,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns)
            : base(collection, table.IsNullThrowArgumentNull(nameof(table)).Table)
        {
            Columns = columns as ReadOnlyCollection<DbColumnDeclaration>;
            if (Columns == null)
            {
                Columns = new List<DbColumnDeclaration>(columns).AsReadOnly();
            }

            From = table ?? throw new System.ArgumentNullException(nameof(table));
        }

        protected internal DbSelectExpression(
            object collection,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns,
            Expression where)
            : this(collection, table, columns, where, null, null) { }

        protected internal DbSelectExpression(
            object collection,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy
            )
            : this(collection, table, columns, where, orderBy, groupBy, false, null, null) { }

        protected internal DbSelectExpression(
            object collection,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression skip,
            Expression take)
            : this(collection, table, columns)
        {
            IsDistinct = isDistinct;
            Where = where;

            OrderBy = orderBy as ReadOnlyCollection<DbOrderByDeclaration>;
            if (OrderBy == null && orderBy != null)
            {
                OrderBy = new List<DbOrderByDeclaration>(orderBy).AsReadOnly();
            }
            GroupBy = groupBy as ReadOnlyCollection<Expression>;
            if (GroupBy == null && groupBy != null)
            {
                GroupBy = new List<Expression>(groupBy).AsReadOnly();
            }
            Take = take;
            Skip = skip;
        }

        public override ExpressionType NodeType => (ExpressionType)DbExpressionType.Select;

        public IReadOnlyCollection<DbColumnDeclaration> Columns { get; }
        public DbTableExpression From { get; set; }
        public new Expression Where { get; }

        public ReadOnlyCollection<DbOrderByDeclaration> OrderBy { get; }
        public ReadOnlyCollection<Expression> GroupBy { get; }
        public bool IsDistinct { get; }
        public Expression Skip { get; set; }
        public Expression Take { get; set; }
        public string QueryText
        {
            get { return DbContext.GenerateSqlFor(this); }
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitSelect(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbSelect(object collection, DbTableExpression table)
        {
            return new DbSelectExpression(collection, table);
        }
    }
}
