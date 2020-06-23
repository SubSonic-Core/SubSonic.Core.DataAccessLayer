using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    using Structure;
    using SubSonic.Infrastructure;
    using SubSonic.Infrastructure.Schema;
    using System.Data.Common;

    public class DbInsertExpression
        : DbExpression
    {
        protected internal DbInsertExpression(DbTableExpression table, IEnumerable<Expression> values)
            : base(DbExpressionType.Insert, table.IsNullThrowArgumentNull(nameof(table)).Type)
        {
            Into = table ?? throw new ArgumentNullException(nameof(table));
            Values = values ?? throw new ArgumentNullException(nameof(values));
            DbParameters = new List<DbParameter>();
        }

        public override ExpressionType NodeType => (ExpressionType)DbExpressionType.Insert;

        public DbTableExpression Into { get; }
        public IEnumerable<Expression> Values { get; }
        public ICollection<DbParameter> DbParameters { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitInsert(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbInsert(DbTableExpression table, IEnumerable<Expression> entities)
        {
            return new DbInsertExpression(table, entities);
        }
    }
}
