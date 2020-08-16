using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using SubSonic.Core.DAL.src;
    using Structure;

    public class DbUpdateExpression
        : DbExpression
    {
        protected internal DbUpdateExpression(DbExpression from, Expression where, IEnumerable<Expression> data)
            : base(DbExpressionType.Update, from.IsNullThrowArgumentNull(nameof(from)).Type)
        {
            if (from is DbTableExpression dbTable)
            {
                From = dbTable;
            }
            else
            {
                throw new ArgumentException(SubSonicErrorMessages.ExpectedDbTableExpression, nameof(from));
            }

            Data = data ?? throw new ArgumentNullException(nameof(data));
            Where = where;
            DbParameters = new List<DbParameter>();
        }

        public DbTableExpression From { get; }

        public Expression Where { get; }

        public ICollection<DbParameter> DbParameters { get; }

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
        public static DbExpression DbUpdate(DbExpression from, Expression where, IEnumerable<Expression> data)
        {
            return new DbUpdateExpression(from, where, data); 
        }

        public static DbExpression DbUpdate(DbExpression from, IEnumerable<Expression> data, string inputTableName)
        {
            if (inputTableName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(inputTableName));
            }

            DbUpdateExpression dbUpdate = (DbUpdateExpression)DbUpdate(from, null, data);

            dbUpdate.From.Joins.Add(DbJoin(JoinType.InnerJoin, from, DbTableType(dbUpdate.From.Model, inputTableName)));

            return dbUpdate;
        }
    }
}
