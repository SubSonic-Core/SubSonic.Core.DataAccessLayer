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
        : DbAliasedExpression
    {
        public DbSelectExpression(TableAlias alias, IEnumerable<DbColumnDeclaration> columns, Expression from)
            : base(DbExpressionType.Select, typeof(Queryable), alias)
        {
            Columns = columns as ReadOnlyCollection<DbColumnDeclaration>;
            if (Columns == null)
            {
                Columns = new List<DbColumnDeclaration>(columns).AsReadOnly();
            }

            From = from ?? throw new System.ArgumentNullException(nameof(from));
        }

        public DbSelectExpression(
            TableAlias alias,
            IEnumerable<DbColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression skip,
            Expression take)
            : this(alias, columns, from)
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
        public DbSelectExpression(
            TableAlias alias,
            IEnumerable<DbColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy
            )
            : this(alias, columns, from, where, orderBy, groupBy, false, null, null)
        {
        }
        public DbSelectExpression(
            TableAlias alias, IEnumerable<DbColumnDeclaration> columns,
            Expression from, Expression where)
            : this(alias, columns, from, where, null, null)
        {
        }
        public IReadOnlyCollection<DbColumnDeclaration> Columns { get; }
        public Expression From { get; set; }
        public new Expression Where { get; }

        public ReadOnlyCollection<DbOrderByDeclaration> OrderBy { get; }
        public ReadOnlyCollection<Expression> GroupBy { get; }
        public bool IsDistinct { get; }
        public Expression Skip { get; set; }
        public Expression Take { get; set; }
        public string QueryText
        {
            get { return TSqlFormatter.Format(this, DbContext.ServiceProvider.GetService<SqlQueryProvider>().Context); }
        }

    }
}
