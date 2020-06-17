using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure.Schema;
    using SubSonic.Linq.Expressions.Structure;
    using System.Collections;
    using System.Collections.Generic;


    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
    public class DbTableExpression
        : DbConstantExpression
    {
        private readonly string _alias;

        protected internal DbTableExpression(IDbEntityModel model, TableAlias alias)
            : this(model.IsNullThrowArgumentNull(nameof(model)).CreateObject(), alias)
        {
            Model = model;
            Joins = new List<DbExpression>();
        }

        protected internal DbTableExpression(DbTableExpression table, string alias)
            : this(table.IsNullThrowArgumentNull(nameof(table)).QueryObject, table.IsNullThrowArgumentNull(nameof(table)).Alias)
        {
            Model = table.IsNullThrowArgumentNull(nameof(table)).Model;
            Joins = new List<DbExpression>();
            _alias = alias;
        }

        public DbTableExpression(object value, TableAlias alias)
            : base(value, alias)
        {
            Alias.IsNotNull(Al => Al.SetTable(this));
        }

        public override ExpressionType NodeType => (ExpressionType)DbExpressionType.Table;

        public IDbEntityModel Model { get; }

        public bool IsNamedAlias => _alias.IsNotNullOrEmpty();

        public IEnumerable<DbColumnDeclaration> Columns => Model.Properties.ToColumnList(this);

        public ICollection<DbExpression> Joins { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitExpression(this);
            }

            return base.Accept(visitor);
        }

        public string QualifiedName
        {
            get => _alias ?? Model?.QualifiedName;
        }

        public override string ToString()
        {
            return "T(" + QualifiedName ?? Type.Name + ")";
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbTable(IDbEntityModel model, TableAlias alias)
        {
            return new DbTableExpression(model, alias);
        }

        public static DbExpression DbTable(DbTableExpression table, string alias)
        {
            return new DbTableExpression(table, alias);
        }

        public static DbExpression DbTable(object value, TableAlias alias)
        {
            return new DbTableExpression(value, alias);
        }
    }
}
