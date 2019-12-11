using SubSonic.Linq.Expressions.Alias;
using System;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    public class DbTableExpression
        : DbAliasedExpression
    {
        private readonly string name;

        public DbTableExpression(Table alias, string name)
            : base(DbExpressionType.Table, typeof(void), alias)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public override string ToString()
        {
            return "T(" + name + ")";
        }
    }
}
