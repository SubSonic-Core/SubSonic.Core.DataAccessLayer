using SubSonic.Linq.Expressions.Alias;
using System;

namespace SubSonic.Linq.Expressions
{
    public abstract class DbAliasedExpression
        : DbExpression
    {
        protected DbAliasedExpression(DbExpressionType nodeType, Type type, TableAlias alias)
            : base(nodeType, type)
        {
            this.Alias = alias;
        }

        public TableAlias Alias { get; }
    }
}
