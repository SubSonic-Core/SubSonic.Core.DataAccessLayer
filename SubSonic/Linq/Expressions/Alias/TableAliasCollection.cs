using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Expressions.Alias
{
    internal class TableAliasCollection
        : IEnumerable<TableAlias>
    {
        private readonly Dictionary<TableAlias, string> __aliases = new Dictionary<TableAlias, string>();

        public string GetAliasName(TableAlias alias)
        {
            string name;

            if(!__aliases.TryGetValue(alias, out name))
            {
                __aliases.Add(alias, alias.UseNameForAlias ? alias.Name : NextAlias);

                name = __aliases[alias];
            }

            return name;
        }

        public string NextAlias => $"T{__aliases.Keys.Count + 1}";

        public void Reset() => __aliases.Clear();

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
