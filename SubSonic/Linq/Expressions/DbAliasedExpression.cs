using SubSonic.Linq.Expressions.Alias;
using System;

namespace SubSonic.Linq.Expressions
{
    public abstract class DbAliasedExpression
        : DbExpression
    {
        Table alias;
        protected DbAliasedExpression(DbExpressionType nodeType, Type type, Table alias)
            : base(nodeType, type)
        {
            this.alias = alias;
        }
        public Table Alias
        {
            get { return alias; }
            set { alias = value; }
        }
    }
}
