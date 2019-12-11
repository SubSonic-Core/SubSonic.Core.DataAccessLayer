namespace SubSonic.Linq.Expressions.Alias
{
    public class Table
    {
        public Table()
        {
        }

        public override string ToString()
        {
            return "A:" + GetHashCode();
        }
    }
}
