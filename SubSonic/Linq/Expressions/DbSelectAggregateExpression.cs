using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbSelectAggregateExpression
        : DbConstantExpression
    {
        private readonly DbSelectExpression select;
        protected internal DbSelectAggregateExpression(DbSelectExpression select, IEnumerable<DbExpression> columns)
            : base(
                  select.IsNullThrowArgumentNull(nameof(select)).QueryObject,
                  select.IsNullThrowArgumentNull(nameof(select)).Type,
                  select.IsNullThrowArgumentNull(nameof(select)).Alias)
        {
            if (select is null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            Columns = new ReadOnlyCollection<DbExpression>(columns.ToList());

            this.select = select;
        }

        public ReadOnlyCollection<DbExpression> Columns { get; }

        public bool IsCte => select.IsCte;

        public DbTableExpression From => select.From;

        public Expression Where => select.Where;

        public ReadOnlyCollection<Expression> GroupBy => select.GroupBy;

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
        public static DbExpression DbSelectAggregate(DbSelectExpression select, IEnumerable<DbExpression> columns)
        {
            return new DbSelectAggregateExpression(select, columns);
        }
    }
}
