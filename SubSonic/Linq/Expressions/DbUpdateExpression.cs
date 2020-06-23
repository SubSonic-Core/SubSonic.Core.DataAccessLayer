using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public class DbUpdateExpression
        : DbExpression
    {
        protected internal DbUpdateExpression(DbExpression table, IEnumerable<Expression> data)
            : base(DbExpressionType.Update, table.IsNullThrowArgumentNull(nameof(table)).Type)
        {
            if (table is DbTableExpression dbTable)
            {
                Table = dbTable;
            }
            else
            {
                throw new ArgumentException(SubSonicErrorMessages.ExpectedDbTableExpression, nameof(table));
            }

            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public DbTableExpression Table { get; }

        public IEnumerable<Expression> Data { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitUpdate(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbUpdate(DbExpression table, IEnumerable<Expression> data)
        {
            return new DbUpdateExpression(table, data);
        }

        public static DbExpression DbUpdate(DbExpression table, IEnumerable<Expression> data, string inputTableName)
        {
            if (inputTableName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(inputTableName));
            }

            DbUpdateExpression dbUpdate = new DbUpdateExpression(table, data);

            dbUpdate.Table.Joins.Add(DbJoin(JoinType.InnerJoin, table, DbTableType(dbUpdate.Table.Model, inputTableName)));

            return dbUpdate;
        }
    }
}
