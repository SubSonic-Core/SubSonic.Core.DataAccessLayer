using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
#if NETSTANDARD2_1
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;
#endif
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SubSonic.Linq
{
    using Collections;
    using Interfaces;
    

    public static class SubSonicAsyncQueryable
    { 
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
        {
            if (source is IAsyncEnumerable<TSource> asyncSource)
            {
                return asyncSource;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> SingleAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> SingleAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleAsync), 
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> SingleOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> SingleOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> FirstAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> FirstAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> FirstOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> FirstOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<IAsyncEnumerable<TSource>> LoadAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<IAsyncEnumerable<TSource>> LoadAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(LoadAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
                return await _source.ProviderAsync
                    .ExecuteAsync<IAsyncEnumerable<TSource>>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                    .ConfigureAwait(false);
            }

            throw Error.NotSupported($"{source}");
        }
        
    }
}
