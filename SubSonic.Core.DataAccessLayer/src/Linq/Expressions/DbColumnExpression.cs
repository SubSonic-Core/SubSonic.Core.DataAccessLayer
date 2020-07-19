using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Alias;
    using Structure;
    using SubSonic.src;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A custom expression node that represents a reference to a column in a SQL query
    /// </summary>
    public class DbColumnExpression 
        : DbExpression, IEquatable<DbColumnExpression>
    {
        protected internal DbColumnExpression(Type type, TableAlias alias, string columnName)
            : base(DbExpressionType.Column, type)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("", nameof(columnName));
            }

            this.Alias = alias;
            this.Name = columnName;
        }

        public TableAlias Alias { get; }

        public string Name { get; }

        public override string ToString()
        {
            return $"{Alias}.C({Name})";
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitExpression(this);
            }

            return base.Accept(visitor);
        }

        public override int GetHashCode()
        {
#if NETSTANDARD2_0
            return Alias.GetHashCode() + Name.GetHashCode();
#elif NETSTANDARD2_1
            return Alias.GetHashCode() + Name.GetHashCode(StringComparison.CurrentCulture);
#endif

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

    public partial class DbExpression
    {
        public static DbExpression DbColumn(Type type, TableAlias alias, string name)
        {
            return new DbColumnExpression(type, alias, name);
        }

        private static Expression BuildColumnExpression(Expression expression, ISubSonicTableProvider tableProvider)
        {
            if (expression is MemberExpression member)
            {
                DbTableExpression table = null;
                DbColumnExpression column = null;

                if (tableProvider.Tables.Any(x =>
                    x.Model.EntityModelType == member.Member.DeclaringType))
                {
                    table = tableProvider.Tables.Single(x =>
                        x.Model.EntityModelType == member.Member.DeclaringType);
                    column = new DbColumnExpression(member.Type, table.Alias, member.Member.Name);
                }
                else
                {
                    throw Error.InvalidOperation(SubSonicErrorMessages.MissingTableReferenceFor.Format(member.Member.DeclaringType.Name));
                }

                return column;
            }
            else if (expression is MethodCallExpression call)
            {
                if (BuildColumnExpression(call.Object, tableProvider) is DbColumnExpression column)
                {
                    return Call(column, call.Method, call.Arguments);
                }
            }

            throw Error.NotSupported();
        }

        public static Expression DbColumn(MemberBinding binding, DbExpression select)
        {
            if (select is ISubSonicTableProvider provider)
            {
                if (binding is MemberAssignment element)
                {
                    if (element.Member is MemberInfo info)
                    {
                        return BuildColumnExpression(element.Expression, provider);
                    }
                }
            }

            throw Error.NotSupported();
        }
    }
}
