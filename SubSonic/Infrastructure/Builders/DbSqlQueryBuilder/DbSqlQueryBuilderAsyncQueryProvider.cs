using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using System.Data.Common;

    public partial class DbSqlQueryBuilder
    {
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            TResult result = default(TResult);

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

                            result = DbContext.Current.ChangeTracking.Where<TResult>(elementType, this, query);
                        }
                        else
                        {
                            if (await Scope.Database.ExecuteScalarAsync(dbQuery, cancellationToken)
                                .ConfigureAwait(false) is TResult scalar)
                            {
                                result = scalar;
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
                result = await ExecuteAsync<TResult>(BuildSelect(call), cancellationToken).ConfigureAwait(true);
            }

            if (result is TResult success)
            {
                return success;
            }
            else
            {
                throw Error.NotImplemented();
            }
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
