using SubSonic.Linq.Expressions.Alias;
using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    public class DbTableExpression
        : DbAliasedExpression
    {
        private readonly string name;

        public DbTableExpression(Type tableType, TableAlias alias, string name)
            : base(DbExpressionType.Table, tableType, alias)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }
            this.name = name;
            Alias.IsNotNull(Al => Al.SetTable(this));
        }

        public string Name
        {
            get { return name; }
        }

        public new ParameterExpression Parameter => Expression.Parameter(Type, Name);

        public override string ToString()
        {
            return "T(" + name + ")";
        }
    }
}
