using System;
using System.Text;

namespace SubSonic.Linq.Expressions.Alias
{
    public class TableAlias
        : BaseAlias
    {
        private int? hash;

        public TableAlias()
            : base()
        {
        }

        public TableAlias(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException("", nameof(name));
            }

            Name = name;
        }

        public DbTableExpression Table { get; private set; }

        internal void SetTable(DbTableExpression table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as TableAlias);
        }

        public bool Equals(TableAlias right)
        {
            return (this == right);
        }

        public override int GetHashCode()
        {
            if(!hash.HasValue)
            {
                ComputeHash();
            }

            return hash.Value;
        }

        public static bool operator ==(TableAlias left, TableAlias right)
        {
            if(left is null && right is null)
            {
                return true;
            }
            else if(left is null || right is null)
            {
                return false;
            }

            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(TableAlias left, TableAlias right)
        {
            return !(left == right);
        }

        private void ComputeHash()
        {
            Random rand = new Random();

            int s = rand.Next(0, 314), t = 159;
            byte[] bytes = Encoding.UTF8.GetBytes(Name);
            hash = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash * s + bytes[i];
                s *= t;
            }
        }
    }
}
