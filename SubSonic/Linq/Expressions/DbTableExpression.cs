using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Infrastructure.Schema;
    using System.Collections.Generic;

    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    public class DbTableExpression
        : DbAliasedExpression
    {
        private readonly IDbEntityModel model;

        public DbTableExpression(IDbEntityModel model)
            : this(model.IsNullThrowArgumentNull(nameof(model)).EntityModelType, model.ToAlias())
        {
            this.model = model;
        }
        public DbTableExpression(Type tableType, TableAlias alias)
            : base(DbExpressionType.Table, tableType, alias)
        {
            Alias.IsNotNull(Al => Al.SetTable(this));
        }

        public IDbEntityModel Model
        {
            get { return model; }
        }

        public IEnumerable<DbColumnDeclaration> Columns => model.Properties.ToColumnList(this);

        public new ParameterExpression Parameter => Expression.Parameter(Type, Model.Name);

        public override string ToString()
        {
            return "T(" + model.IsNotNull(M => M.QualifiedName, Type.Name) + ")";
        }
    }
}
