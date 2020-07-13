using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic
{
    public interface IAsyncSubSonicQueryProvider
        : IQueryProvider
    {
#if NETSTANDARD2_1
        /// <summary>
        /// Execute a method call expression asynchronously
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> ExecuteMethodAsync<TResult>([NotNull] MethodCallExpression expression,[NotNull] CancellationToken cancellationToken);
        /// <summary>
        /// Execute the method call built by <see cref="SubSonicAsyncQueryable.LoadAsync{TResult}"
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        IAsyncEnumerable<TResult> ExecuteLoadAsync<TResult>([NotNull] MethodCallExpression call, [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
#endif
        /// <summary>
        /// Executes the expression asynchronously 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken);
#elif NETSTANDARD2_1
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        IAsyncEnumerable<object> ExecuteAsync([NotNull] Expression expression, [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
#endif
        /// <summary>
        /// Executes the expression asynchronously 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Expression"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        Task<TResult> ExecuteAsync<TResult>(Expression Expression, CancellationToken cancellationToken);
#elif NETSTANDARD2_1
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        IAsyncEnumerable<TResult> ExecuteAsync<TResult>([NotNull] Expression Expression, [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
#endif
    }
}
