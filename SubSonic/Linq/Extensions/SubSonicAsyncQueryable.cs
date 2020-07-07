using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
#if NETSTANDARD2_1
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;
#endif
using System.Reflection;

namespace SubSonic.Linq
{
    using Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public static class SubSonicAsyncQueryable
    { 
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IAsyncSubSonicQueryable<TSource> AsAsyncSubSonicQueryable<TSource>(this IQueryable<TSource> source)
        {
            if (source is IAsyncSubSonicQueryable<TSource> asyncSource)
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
        public static async Task<TSource> SingleAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleAsync),
                new[] {
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
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
        public static async Task<TSource> SingleAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
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
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> SingleOrDefaultAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleOrDefaultAsync),
                new[] {
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
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
        public static async Task<TSource> SingleOrDefaultAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
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
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> FirstAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstAsync),
                new[] {
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
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
        public static async Task<TSource> FirstAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
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
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<TSource> FirstOrDefaultAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstOrDefaultAsync),
                new[] {
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
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
        public static async Task<TSource> FirstOrDefaultAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.Requires(!(predicate is null), $"Parameter {nameof(predicate)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
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
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<TSource>(Expression.Call(method, new[] { source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static async Task<ISubSonicCollection<TSource>> LoadAsync<TSource>([NotNull] this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(!(source is null), $"Parameter {nameof(source)} cannot be null.");
            Contract.EndContractBlock();
#elif NETSTANDARD2_0
        public static async Task<ISubSonicCollection<TSource>> LoadAsync<TSource>(this IAsyncSubSonicQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
#endif
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(LoadAsync),
                new[] {
                    typeof(IAsyncSubSonicQueryable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            return await source.AsyncProvider
                .ExecuteAsync<ISubSonicCollection<TSource>>(Expression.Call(method, new[] { source.Expression, Expression.Constant(cancellationToken) }), cancellationToken)
                .ConfigureAwait(false);
        }
        
    }
}
