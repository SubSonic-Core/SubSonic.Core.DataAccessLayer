using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Builders
{
    using src;
    using Collections;
    using Linq;
    using Linq.Expressions;
    using Logging;
    using System.Globalization;
    using SubSonic.src.Collections;

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

        public TResult ExecuteMethod<TResult>(MethodCallExpression call)
        {
            if (call is null)
            {
                throw Error.ArgumentNull(nameof(call));
            }

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
                        switch (lambda.Body.NodeType)
                        {
                            case ExpressionType.MemberAccess:
                                dbSelect = (DbSelectExpression)BuildSelect(dbSelect, dbSelect.Columns.Where(x => x.PropertyName.Equals(lambda.GetProperty().Name, StringComparison.Ordinal)));
                                break;
                            default:
                                where = BuildWhere(dbSelect, lambda);
                                break;
                        }
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
            else if (call.Method.Name.In(nameof(Queryable.Count), nameof(Queryable.LongCount), nameof(Queryable.Min), nameof(Queryable.Max), nameof(Queryable.Sum), nameof(Queryable.Average)))
            {
                if (BuildSelect(dbSelect, where) is DbSelectExpression select)
                {
                    if (!Enum.TryParse<AggregateType>(call.Method.Name, out AggregateType aggregateType))
                    {
                        throw Error.NotSupported(SubSonicErrorMessages.MethodNotSupported.Format(call.Method.Name));
                    }

                    Expression argument = null;

                    if (select.Columns.Count > 1)
                    {
                        argument = select.Columns.First(x => x.Property.IsPrimaryKey).Expression;
                    }
                    else
                    {
                        argument = select.Columns.Single().Expression;
                    }

                    TResult result = Execute<TResult>(DbExpression.DbSelectAggregate(select, new[]
                    {
                            DbExpression.DbAggregate(typeof(TResult), aggregateType, argument)
                    }));

                    if (select.Take is ConstantExpression take)
                    {
                        if (result.IsIntGreaterThan(take.Value))
                        {
                            return (TResult)Convert.ChangeType(take.Value, typeof(TResult), CultureInfo.InvariantCulture);
                        }
                    }

                    return result;
                }
            }
            else if (call.Method.Name.In(nameof(Queryable.Any)))
            {
                object entities = Execute(BuildSelect(dbSelect, where));
                bool result = false;

                if (entities is ISubSonicCollection collection)
                {
                    result = collection.Count > 0;
                }

                return (TResult)Convert.ChangeType(result, typeof(TResult), CultureInfo.InvariantCulture);
            }

            throw Error.NotSupported(SubSonicErrorMessages.ExpressionNotSupported.Format(call.Method.Name));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression is MethodCallExpression call)
            {   // execution request originates from the System.Linq namespace
                return ExecuteMethod<TResult>(call);
            }
            else if (expression is DbExpression query)
            {   // execution request is from the subsonic namespace
                using (SharedDbConnectionScope Scope = SubSonicContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    CmdBehavior = typeof(TResult).IsEnumerable() ? CommandBehavior.Default : CommandBehavior.SingleRow;

                    Type elementType = typeof(TResult).GetQualifiedType();

                    bool isEntityModel = SubSonicContext.DbModel.IsEntityModelRegistered(elementType);

                    if (!isEntityModel ||
                        SubSonicContext.Current.ChangeTracking.Count(elementType, query) == 0)
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
                                        SubSonicContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
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
                        return SubSonicContext.Current.ChangeTracking.Where<TResult>(elementType, this, query);
                    }
                    else
                    {
                        logger.LogDebug(SubSonicErrorMessages.NoDataAvailable);

                        return default(TResult);
                    }
                }
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
                using (SharedDbConnectionScope Scope = SubSonicContext.ServiceProvider.GetService<SharedDbConnectionScope>())
                {
                    IDbQuery dbQuery = ToQuery(query);

                    try
                    {
                        Type elementType = query.Type.GetQualifiedType();

                        bool isEntityModel = SubSonicContext.DbModel.IsEntityModelRegistered(elementType);

                        Scope.Connection.Open();

                        if (isEntityModel)
                        {
                            using (DbDataReader reader = Scope.Database.ExecuteReader(dbQuery))
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        SubSonicContext.Current.ChangeTracking.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
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
                            return SubSonicContext.Current.ChangeTracking.Where(elementType, this, query);
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
