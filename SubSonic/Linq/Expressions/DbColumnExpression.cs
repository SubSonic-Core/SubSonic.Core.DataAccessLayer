using SubSonic.Linq.Expressions.Alias;
using System;

namespace SubSonic.Linq.Expressions
{
    /// <summary>
    /// A custom expression node that represents a reference to a column in a SQL query
    /// </summary>
    public class DbColumnExpression : DbExpression, IEquatable<DbColumnExpression>
    {
        public DbColumnExpression(Type type, Table alias, string columnName)
            : base(DbExpressionType.Column, type)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("", nameof(columnName));
            }

            this.Alias = alias ?? throw new ArgumentNullException(nameof(alias));
            this.Name = columnName;
        }

        public Table Alias { get; }

        public string Name { get; }

        public override string ToString()
        {
            return Alias.ToString() + ".C(" + Name + ")";
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode() + Name.GetHashCode(StringComparison.CurrentCulture);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DbColumnExpression);
        }

        public bool Equals(DbColumnExpression other)
        {
            return other != null &&
                ((this == other) || (Alias == other.Alias && Name == other.Name));
        }

        public static bool operator ==(DbColumnExpression left, DbColumnExpression right)
        {
            if (left is null && right is null)
            {
                return true;
            } 
            else if (left is null || right is null)
            {
                return false;
            }

            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(DbColumnExpression left, DbColumnExpression right)
        {
            return !(left == right);
        }
    }
}
