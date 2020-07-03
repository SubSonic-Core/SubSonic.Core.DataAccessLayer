using SubSonic.Linq.Expressions.Alias;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure;
    using Structure;
    using System;

    /// <summary>
    /// A custom expression node used to represent a SQL SELECT expression
    /// </summary>
    public class DbSelectExpression
        : DbConstantExpression
    {
        protected internal DbSelectExpression(
            object collection,
            Type type,
            DbTableExpression table)
            : this(collection, type, table, table.IsNullThrowArgumentNull(nameof(table)).Columns) { }

        protected internal DbSelectExpression(
            object collection,
            Type type,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns)
            : base(collection, type, table.IsNullThrowArgumentNull(nameof(table)).Alias)
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
            Type type,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns,
            Expression where)
            : this(collection, type, table, columns, where, null, null) { }

        protected internal DbSelectExpression(
            object collection,
            Type type,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy
            )
            : this(collection, type, table, columns, where, orderBy, groupBy, false, null) { }

        protected internal DbSelectExpression(
            object collection,
            Type type,
            DbTableExpression table,
            IEnumerable<DbColumnDeclaration> columns,
            Expression where,
            IEnumerable<DbOrderByDeclaration> orderBy,
            IEnumerable<Expression> groupBy,
            bool isDistinct,
            Expression take)
            : this(collection, type, table, columns)
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
        }

        public bool IsCte { get; set; }

        public override ExpressionType NodeType => (ExpressionType)DbExpressionType.Select;

        public IReadOnlyCollection<DbColumnDeclaration> Columns { get; }
        public DbTableExpression From { get; set; }
        public Expression Where { get; }

        public ReadOnlyCollection<DbOrderByDeclaration> OrderBy { get; }
        public ReadOnlyCollection<Expression> GroupBy { get; }
        public bool IsDistinct { get; }
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

        public override string ToString()
        {
            return QueryText;
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbSelect(object collection, Type type, DbTableExpression table)
        {
            return new DbSelectExpression(collection, type, table);
        }

        public static DbExpression DbSelect(object collection, Type type, DbTableExpression table, IEnumerable<DbColumnDeclaration> columns, Expression where, IEnumerable<DbOrderByDeclaration> orderBy, bool cte = false)
        {
            return new DbSelectExpression(collection, type, table, columns, where, orderBy, null)
            {
                IsCte = cte
            };
        }

        public static DbExpression DbSelect(DbSelectExpression select, DbExpression where)
        {
            if (select is null)
            {
                throw Error.ArgumentNull(nameof(select));
            }

            return new DbSelectExpression(select.QueryObject, select.Type, select.From, select.Columns, where ?? select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Take);
        }

        public static DbExpression DbSelect(DbSelectExpression select, DbJoinExpression join)
        {
            if (select is null)
            {
                throw new System.ArgumentNullException(nameof(select));
            }

            if (join is null)
            {
                throw new System.ArgumentNullException(nameof(join));
            }

            DbTableExpression dbTable = (DbTableExpression)DbExpression.DbTable(select.From.Model, select.From.Alias);

            foreach (DbExpression existing_joins in select.From.Joins)
            {
                dbTable.Joins.Add(existing_joins);
            }

            dbTable.Joins.Add(join);

            return new DbSelectExpression(select.QueryObject, select.Type, dbTable, select.Columns, select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Take);
        }
    }
}
