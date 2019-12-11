namespace SubSonic.Linq.Expressions
{
    public class DbExistsExpression
        : DbSubQueryExpression
    {
        public DbExistsExpression(DbSelectExpression select)
           : base(DbExpressionType.Exists, typeof(bool), select)
        {
        }
    }
}
