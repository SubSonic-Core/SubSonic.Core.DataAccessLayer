using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SubSonic.Infrastructure.Builders
{
    using Logging;
    using Linq.Expressions;
    using System.Threading.Tasks;
    using SubSonic.Linq;

    public partial class DbSqlQueryBuilder
    {
        [DefaultValue(CommandBehavior.Default)]
        protected CommandBehavior CmdBehavior { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                return new SubSonicCollection(DbEntity.EntityModelType, this, BuildQuery(expression));
            }
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return new SubSonicCollection<TEntity>(this, BuildQuery(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression is DbExpression query)
            {   // execution request is from the subsonic namespace
                using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    CmdBehavior = typeof(TResult).IsEnumerable() ? CommandBehavior.Default : CommandBehavior.SingleRow;

                    Type elementType = typeof(TResult).GetQualifiedType();

                    bool isEntityModel = DbContext.DbModel.IsEntityModelRegistered(elementType);

                    if (!isEntityModel ||
                        DbContext.Current.ChangeTracking.Count(elementType, query) == 0)
                    {
                        IDbQuery dbQuery = ToQuery(query);

                        try
                        {
                            Scope.Connection.Open();

                            if (isEntityModel)
                            {
                                using (DbDataReader reader = Scope.Database.ExecuteReader(dbQuery))
                                {
                                    while (reader.Read())
                                    {
                                        DbContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                                    }
                                }
                            }
                            else if (CmdBehavior == CommandBehavior.SingleRow)
                            {
                                if (Scope.Database.ExecuteScalar(dbQuery) is TResult result)
                                {
                                    return result;
                                }
                            }
                        }
                        finally
                        {
                            dbQuery.CleanUpParameters();

                            Scope.Connection.Close();
                        }
                    }

                    if (isEntityModel)
                    {
                        return DbContext.Current.ChangeTracking.Where<TResult>(elementType, this, query);
                    }
                    else
                    {
                        logger.LogDebug(SubSonicErrorMessages.NoDataAvailable);

                        return default(TResult);
                    }
                }
            }
            else if (expression is MethodCallExpression call)
            {   // execution request originates from the System.Linq namespace

                DbSelectExpression dbSelect = null;
                Expression where = null;

                for (int i = 0, n = call.Arguments.Count; i < n; i++)
                {
                    if (call.Arguments[i] is DbSelectExpression select)
                    {
                        dbSelect = select;
                    }
                    else if (call.Arguments[i] is UnaryExpression unary)
                    {
                        if (unary.Operand is LambdaExpression lambda)
                        {
                            where = BuildWhere(dbSelect, lambda);
                        }
                    }
                }

                if (call.Method.Name.In(nameof(Queryable.Single), nameof(Queryable.SingleOrDefault), nameof(Queryable.First), nameof(Queryable.FirstOrDefault)))
                {
                    object result = Execute<TResult>(BuildSelect(dbSelect, where));

                    if (result is TResult matched)
                    {
                        return matched;
                    }
                    else if (result is IEnumerable<TResult> enumerable)
                    {
                        return enumerable.Any() ? enumerable.ElementAt(0) : default(TResult);
                    }
#if NETSTANDARD2_0
                    else if (call.Method.Name.Contains("Default"))
#elif NETSTANDARD2_1
                    else if (call.Method.Name.Contains("Default", StringComparison.CurrentCulture))
#endif
                    {
                        return default(TResult);
                    }
                    else
                    {
                        throw Error.InvalidOperation($"Method {call.Method.Name} expects data.");
                    }
                }
                else if (call.Method.Name.In(nameof(Queryable.Count)))
                {
                    if (BuildSelect(dbSelect, where) is DbSelectExpression select)
                    {
                        return Execute<TResult>(DbExpression.DbSelectAggregate(select, new[]
                        {
                            DbExpression.DbAggregate(typeof(TResult), AggregateType.Count, select.Columns.First(x => x.Property.IsPrimaryKey).Expression)
                        }));
                    }
                }
                
                throw Error.NotSupported(SubSonicErrorMessages.ExpressionNotSupported.Format(call.Method.Name));
            }

            throw new NotSupportedException(expression.ToString());
        }

        public object Execute(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            if (expression is DbExpression query)
            {
                using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    IDbQuery dbQuery = ToQuery(query);

                    try
                    {
                        Type elementType = query.Type.GetQualifiedType();

                        bool isEntityModel = DbContext.DbModel.IsEntityModelRegistered(elementType);

                        Scope.Connection.Open();

                        if (isEntityModel)
                        {
                            using (DbDataReader reader = Scope.Database.ExecuteReader(dbQuery))
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        DbContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                                    }
                                }
                            }
                        }
                        else if (CmdBehavior == CommandBehavior.SingleRow)
                        {
                            return Scope.Database.ExecuteScalar(dbQuery);
                        }

                        if (isEntityModel)
                        {
                            return DbContext.Current.ChangeTracking.Where(elementType, this, query);
                        }
                        else
                        {
                            logger.LogDebug(SubSonicErrorMessages.NoDataAvailable);

                            return Array.Empty<object>();
                        }
                    }
                    finally
                    {
                        dbQuery.CleanUpParameters();

                        Scope.Connection.Close();
                    }
                }
            }
            else
            {
                throw Error.NotSupported($"{expression} Not Supported");
            }
        }
    }
}
