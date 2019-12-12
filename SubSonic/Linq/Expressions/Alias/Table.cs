namespace SubSonic.Linq.Expressions.Alias
{
    public class Table
        : BaseAlias
    {
        public Table()
            : base()
        {
        }

        public Table(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException("", nameof(name));
            }

            Name = name;
        }
    }
}
