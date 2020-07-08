using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Common;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public partial class DbSqlQueryBuilder
    {
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            TResult @return = default(TResult);

            if (expression is DbExpression query)
            {
                using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    IDbQuery dbQuery = ToQuery(query);

                    try
                    {
                        await Scope.Connection.OpenAsync(cancellationToken)
                            .ConfigureAwait(false);

                        Type elementType = query.Type.GetQualifiedType();

                        bool isEntityModel = DbContext.DbModel.IsEntityModelRegistered(elementType);

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
                                        DbContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                                    }
                                }
                            }

                            @return = DbContext.Current.ChangeTracking.Where<TResult>(elementType, this, query);
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

                        if (!(Scope.Connection is null))
                        {
#if NETSTANDARD2_1
                            await Scope.Connection.CloseAsync()
                                .ConfigureAwait(false);
#elif NETSTANDARD2_0
                            Scope.Connection.Close();
#endif
                        }
                    }
                }
            }
            else if (expression is MethodCallExpression call)
            {
                if (!call.Type.GenericTypeArguments[0].IsEnumerable())
                {
                    IEnumerable<TResult> result = await ExecuteAsync<IEnumerable<TResult>>(BuildSelect(call), cancellationToken).ConfigureAwait(true);

                    if (result?.Count() > 0)
                    {
#if NETSTANDARD2_0
                    if (call.Method.Name.Contains("Single"))
#elif NETSTANDARD2_1
                        if (call.Method.Name.Contains("Single", StringComparison.Ordinal))
#endif
                        {
                            if (result.Count() > 1)
                            {
                                throw Error.InvalidOperation($"{call.Method.Name} does not allow more than one result");
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
#if NETSTANDARD2_0
                if (!call.Method.Name.Contains("OrDefault"))
#elif NETSTANDARD2_1
                if (!call.Method.Name.Contains("OrDefault", StringComparison.Ordinal))
#endif
                {
                    throw Error.InvalidOperation($"{call.Method.Name} does not allow nulls to be returned");
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
    }
}
