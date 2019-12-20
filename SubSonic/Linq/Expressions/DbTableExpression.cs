using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure.Schema;
    using SubSonic.Linq.Expressions.Structure;
    using System.Collections.Generic;

    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    public class DbTableExpression
        : DbConstantExpression
    {
        protected internal DbTableExpression(IDbEntityModel model)
            : this(model.IsNullThrowArgumentNull(nameof(model)).CreateObject(), model.ToAlias())
        {
            Model = model;
        }
        public DbTableExpression(object value, TableAlias alias)
            : base(value, alias)
        {
            Table.IsNotNull(Al => Al.SetTable(this));
        }

        public override ExpressionType NodeType => (ExpressionType)DbExpressionType.Table;

        public IDbEntityModel Model { get; }

        public IEnumerable<DbColumnDeclaration> Columns => Model.Properties.ToColumnList(this);

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitExpression(this);
            }

            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return "T(" + Model.IsNotNull(M => M.QualifiedName, Type.Name) + ")";
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbTable(IDbEntityModel model)
        {
            return new DbTableExpression(model);
        }

        public static DbExpression DbTable(object value, TableAlias alias)
        {
            return new DbTableExpression(value, alias);
        }
    }
}
