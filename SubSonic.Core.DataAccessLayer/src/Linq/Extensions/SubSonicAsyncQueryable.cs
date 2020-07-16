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
        public static Task<TSource> SingleAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }
            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
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
        public static Task<TSource> SingleAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleAsync), 
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
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
        public static Task<TSource> SingleOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
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
        public static Task<TSource> SingleOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(SingleOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
            }

            throw Error.NotSupported($"{source}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static Task<TResult> FirstAsync<TResult>([NotNull] this IAsyncEnumerable<TResult> source, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TResult> FirstAsync<TResult>(this IAsyncEnumerable<TResult> source, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TResult)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TResult> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TResult>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TResult>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
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
        public static Task<TSource> FirstAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
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
        public static Task<TSource> FirstOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
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
        public static Task<TSource> FirstOrDefaultAsync<TSource>([NotNull] this IAsyncEnumerable<TSource> source, [NotNull] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (predicate is null)
            {
                throw Error.ArgumentNull(nameof(predicate));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(FirstOrDefaultAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TSource)),
                    predicate.GetType(),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TSource> _source)
            {
#if NETSTANDARD2_1
                return _source.ProviderAsync
                    .ExecuteMethodAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                return _source.ProviderAsync
                    .ExecuteAsync<TSource>(Expression.Call(method, new[] { _source.Expression, predicate, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
            }

            throw Error.NotSupported($"{source}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
#if NETSTANDARD2_1
        public static IAsyncEnumerable<TResult> LoadAsync<TResult>([NotNull] this IAsyncEnumerable<TResult> source, CancellationToken cancellationToken = default(CancellationToken))
#elif NETSTANDARD2_0
        public static Task<IAsyncEnumerable<TResult>> LoadAsync<TResult>(this IAsyncEnumerable<TResult> source, CancellationToken cancellationToken = default(CancellationToken))
#endif
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            MethodInfo method = typeof(SubSonicAsyncQueryable).GetGenericMethod(nameof(LoadAsync),
                new[] {
                    typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TResult)),
                    typeof(CancellationToken)
                });

            if (source is IAsyncSubSonicQueryable<TResult> _source)
            {
                return _source.ProviderAsync
#if NETSTANDARD2_1
                    .ExecuteLoadAsync<TResult>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#elif NETSTANDARD2_0
                    .ExecuteAsync<IAsyncEnumerable<TResult>>(Expression.Call(method, new[] { _source.Expression, Expression.Constant(cancellationToken) }), cancellationToken);
#endif
            }

            throw Error.NotSupported($"{source}");
        }
        
    }
}
