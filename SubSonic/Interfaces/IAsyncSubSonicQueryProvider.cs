using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SubSonic.Interfaces
{
    public interface IAsyncSubSonicQueryProvider
        : IQueryProvider
    {
        /// <summary>
        /// Executes the expression asynchronously 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<object> ExecuteAsync(Expression expression);
        /// <summary>
        /// Executes the expression asynchronously 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Expression"></param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync<TResult>(Expression Expression);
    }
}
