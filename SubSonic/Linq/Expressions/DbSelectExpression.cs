using SubSonic;
using SubSonic.Linq.Expressions.Alias;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ReadOnlyCollection<DbColumnDeclaration> columns;
        bool isDistinct;
        Expression from;
        Expression where;
        ReadOnlyCollection<DbOrderByDeclaration> orderBy;
        ReadOnlyCollection<Expression> groupBy;
        Expression take;
        Expression skip;

        public DbSelectExpression(Table alias, Expression from)
            : base(DbExpressionType.Select, typeof(void), alias)
        {
            From = from ?? throw new System.ArgumentNullException(nameof(from));
        }

        public DbSelectExpression(
            Table alias,
            IEnumerable<DbColumnDeclaration> columns,
            Expression from,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression skip,
            Expression take)
            : base(DbExpressionType.Select, typeof(void), alias)
        {
            this.columns = columns as ReadOnlyCollection<DbColumnDeclaration>;
            if (this.columns == null)
            {
                this.columns = new List<DbColumnDeclaration>(columns).AsReadOnly();
            }
            this.isDistinct = isDistinct;
            this.from = from;
            this.where = where;
            this.orderBy = orderBy as ReadOnlyCollection<DbOrderByDeclaration>;
            if (this.orderBy == null && orderBy != null)
            {
                this.orderBy = new List<DbOrderByDeclaration>(orderBy).AsReadOnly();
            }
            this.groupBy = groupBy as ReadOnlyCollection<Expression>;
            if (this.groupBy == null && groupBy != null)
            {
                this.groupBy = new List<Expression>(groupBy).AsReadOnly();
            }
            this.take = take;
            this.skip = skip;
        }
        public DbSelectExpression(
            Table alias,
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
            Table alias, IEnumerable<DbColumnDeclaration> columns,
            Expression from, Expression where
            )
            : this(alias, columns, from, where, null, null)
        {
        }
        public ReadOnlyCollection<DbColumnDeclaration> Columns
        {
            get { return columns; }
        }
        public Expression From
        {
            get { return from; }
            set { from = value; }
        }
        public Expression Where
        {
            get { return where; }
        }
        public ReadOnlyCollection<DbOrderByDeclaration> OrderBy
        {
            get { return orderBy; }
        }
        public ReadOnlyCollection<Expression> GroupBy
        {
            get { return groupBy; }
        }
        public bool IsDistinct
        {
            get { return isDistinct; }
        }
        public Expression Skip
        {
            get { return skip; }
            set { skip = value; }
        }
        public Expression Take
        {
            get { return take; }
            set { take = value; }
        }
        public string QueryText
        {
            get { return TSqlFormatter.Format(this, DbContext.ServiceProvider.GetService<SqlQueryProvider>().SqlContext); }
        }

    }
}
