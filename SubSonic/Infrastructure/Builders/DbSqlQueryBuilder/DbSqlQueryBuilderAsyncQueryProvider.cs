using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;
#endif

namespace SubSonic.Infrastructure.Builders
{
    using Linq.Expressions;
    using SubSonic.Linq;

    public partial class DbSqlQueryBuilder
    {
#if NETSTANDARD2_0
        /// <summary>
        /// Execute a database call against the db
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            TResult @return = default(TResult);

            if (expression is DbExpression query)
            {
                using (SharedDbConnectionScope Scope = SubSonicContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    IDbQuery dbQuery = ToQuery(query);

                    try
                    {
                        await Scope.Connection.OpenAsync(cancellationToken)
                            .ConfigureAwait(false);

                        Type elementType = query.Type.GetQualifiedType();

                        bool isEntityModel = SubSonicContext.DbModel.IsEntityModelRegistered(elementType);

                        if (isEntityModel)
                        {
                            using (DbDataReader reader = await Scope.Database
                                .ExecuteReaderAsync(dbQuery, cancellationToken)
                                .ConfigureAwait(false))
                            {
                                if (reader.HasRows)
                                {
                                    while (await reader.ReadAsync().ConfigureAwait(false))
                                    {
                                        SubSonicContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                                    }
                                }
                            }

                            @return = SubSonicContext.Current.ChangeTracking.Where<TResult>(elementType, this, query);
                        }
                        else
                        {
                            if (await Scope.Database.ExecuteScalarAsync(dbQuery, cancellationToken)
                                .ConfigureAwait(false) is TResult scalar)
                            {
                                @return = scalar;
                            }
                        }
                    }
                    finally
                    {
                        dbQuery.CleanUpParameters();

                        Scope.Connection.Close();
                    }
                }
            }
            else if (expression is MethodCallExpression call)
            {
                Type callType = call.Type.GenericTypeArguments[0];

                if (!(callType.IsEnumerable() ||
                      callType.IsAsyncEnumerable()))
                {
                    IEnumerable<TResult> result = await ExecuteAsync<IEnumerable<TResult>>(BuildSelect(call), cancellationToken).ConfigureAwait(true);

                    if (result?.Count() > 0)
                    {
                        if (call.Method.Name.Contains("Single"))
                        {
                            if (result.Count() > 1)
                            {
                                throw Error.InvalidOperation(SubSonicErrorMessages.MethodFoundMoreThanOneResult.Format(call.Method.Name));
                            }
                        }

                        @return = result.ElementAt(0);
                    }
                }
                else
                {
                    @return = await ExecuteAsync<TResult>(BuildSelect(call), cancellationToken).ConfigureAwait(true);
                }
            }

            if (@return is TResult success)
            {
                return success;
            }
            else if (expression is MethodCallExpression call)
            {
                if (!call.Method.Name.Contains("OrDefault"))
                {
                    throw Error.InvalidOperation(SubSonicErrorMessages.MethodDoesNotAllowNullValue.Format(call.Method.Name));
                }

                return @return;
            }
            else
            {
                return @return;
            }

            throw Error.NotImplemented();
        }

        public async Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }
#elif NETSTANDARD2_1
        public async Task<TResult> ExecuteMethodAsync<TResult>([NotNull] MethodCallExpression call, [NotNull] CancellationToken cancellationToken = default)
        {
            if (call is null)
            {
                throw Error.ArgumentNull(nameof(call));
            }

            TResult result = default;

            await foreach(TResult entity in ExecuteAsync<TResult>(BuildSelect(call), cancellationToken))
            {
                if (!(result is null))
                {
                    if (call.Method.Name.Contains("Single", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SubSonicErrorMessages.MethodFoundMoreThanOneResult.Format(call.Method.Name));
                    }
                }

                if (entity is TResult success)
                {
                    result = success;

                    if (call.Method.Name.Contains("First", StringComparison.Ordinal))
                    {
                        break;
                    }
                }

                if (entity is null)
                {
                    if (call.Method.Name.Contains("Single", StringComparison.Ordinal) ||
                        call.Method.Name.Contains("First", StringComparison.Ordinal))
                    {
                        break;
                    }
                }
            }

            if (result is null && !call.Method.Name.Contains("OrDefault", StringComparison.Ordinal))
            {
                throw Error.InvalidOperation(SubSonicErrorMessages.MethodDoesNotAllowNullValue.Format(call.Method.Name));
            }

            return result;
        }

        public IAsyncEnumerable<TResult> ExecuteLoadAsync<TResult>([NotNull] MethodCallExpression call, CancellationToken cancellationToken = default)
        {
            if (call is null)
            {
                throw Error.ArgumentNull(nameof(call));
            }

            if (call.Method.Name.Equals(nameof(SubSonicAsyncQueryable.LoadAsync), StringComparison.Ordinal))
            {
                return ExecuteAsync<TResult>(BuildSelect(call), cancellationToken);
            }
            else
            {
                throw Error.NotSupported(SubSonicErrorMessages.MethodNotSupported.Format(call.Method.Name));
            }
        }

        public async IAsyncEnumerable<TResult> ExecuteAsync<TResult>([NotNull] Expression query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            using SharedDbConnectionScope Scope = SubSonicContext.Current.UseSharedDbConnection();
            IDbQuery dbQuery = ToQuery(query);

            Type elementType = query.Type.GetQualifiedType();

            bool isEntityModel = SubSonicContext.DbModel.IsEntityModelRegistered(elementType);

            try
            {
                if (isEntityModel)
                {
                    using (DbDataReader reader = await Scope.Database
                        .ExecuteReaderAsync(dbQuery, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                if (reader.ActivateAndLoadInstanceOf(elementType) is TResult entity)
                                {
                                    SubSonicContext.Current.ChangeTracking.Add(elementType, entity);

                                    yield return entity;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (await Scope.Database.ExecuteScalarAsync(dbQuery, cancellationToken)
                        .ConfigureAwait(false) is TResult scalar)
                    {
                        yield return scalar;
                    }
                }
            }
            finally
            {
                await Scope.Connection.CloseAsync().ConfigureAwait(true);

                dbQuery.CleanUpParameters();
            }
        }

        public async IAsyncEnumerable<object> ExecuteAsync([NotNull] Expression expression, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return default;
        }
#endif
    }
}
