using System.Globalization;

namespace SubSonic.Linq.Expressions.Alias
{
    public class Table
    {
        public Table()
        {
        }

        public Table(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException("", nameof(name));
            }

            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return $"A:{Name ?? GetHashCode().ToString(CultureInfo.CurrentCulture)}";
        }
    }
}
