using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure.Builders
{
    public partial class DbSqlQueryBuilder
    {
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }

        public async Task<object> ExecuteAsync(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }
    }
}
