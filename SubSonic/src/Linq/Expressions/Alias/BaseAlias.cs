using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SubSonic.Linq.Expressions.Alias
{
    public class BaseAlias
    {
        protected BaseAlias() {
        }

        public string Name { get; protected set; }

        public bool UseNameForAlias { get; protected set; }

        public override string ToString()
        {
            return Name ?? $"A:{GetHashCode().ToString(CultureInfo.CurrentCulture)}";
        }
    }
}
