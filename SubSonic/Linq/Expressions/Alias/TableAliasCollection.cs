using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Expressions.Alias
{
    internal class TableAliasCollection
        : IEnumerable<TableAlias>
    {
        [ThreadStatic]
        private static Dictionary<TableAlias, string> __aliases = new Dictionary<TableAlias, string>();

        public static string GetAliasName(TableAlias alias)
        {
            string name;

            if(!__aliases.TryGetValue(alias, out name))
            {
                __aliases.Add(alias, NextAlias);

                name = __aliases[alias];
            }

            return name;
        }

        public static string NextAlias => $"T{__aliases.Keys.Count + 1}";

        public TableAliasCollection() { }

        public IEnumerator<TableAlias> GetEnumerator()
        {
            return __aliases.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)__aliases.Keys).GetEnumerator();
        }
    }
}
