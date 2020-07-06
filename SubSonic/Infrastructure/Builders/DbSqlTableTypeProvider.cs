using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using Logging;
    using Schema;

    public class DbSqlTableTypeProvider
        : ISubSonicQueryProvider
    {
        private readonly string tableTypeName;
        private readonly Type elementType;
        private readonly ISubSonicLogger logger;

        public DbSqlTableTypeProvider(string tableTypeName, Type elementType, ISubSonicLogger logger = null)
        {
            this.tableTypeName = tableTypeName ?? throw new ArgumentNullException(nameof(tableTypeName));
            this.elementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            this.logger = logger ?? DbContext.ServiceProvider.GetService<ISubSonicLogger<DbSqlTableTypeProvider>>();

            if (DbContext.DbModel.TryGetEntityModel(elementType.GetQualifiedType(), out IDbEntityModel model))
            {
                DbEntity = model;
                DbTable = DbEntity.GetTableType(tableTypeName);
            }
        }

        public IDbEntityModel DbEntity { get; }

        public DbTableExpression DbTable { get; }

        public Expression BuildJoin(JoinType type, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public Expression BuildLambda(Expression body, LambdaType callType, params string[] properties)
        {
            throw new NotImplementedException();
        }

        public Expression BuildLogicalBinary(Expression eBody, string name, object value, DbComparisonOperator op, DbGroupOperator group)
        {
            throw new NotImplementedException();
        }

        public Expression BuildLogicalIn(Expression body, PropertyInfo property, IQueryable queryable, DbGroupOperator group)
        {
            throw new NotImplementedException();
        }

        public Expression BuildLogicalIn(Expression body, PropertyInfo property, IEnumerable<Expression> values, DbGroupOperator group)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(Expression eSelect, Expression eWhere)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(Expression eSelect, bool isDistinct)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(Expression eSelect, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(Expression expression, IDbEntityProperty property)
        {
            if (property.IsNull())
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (expression is DbSelectExpression select)
            {
                return new DbSelectExpression(
                    select.QueryObject, 
                    typeof(IQueryable<>).MakeGenericType(property.PropertyType),
                    select.From,
                    select.Columns.Where(x => x.PropertyName.Equals(property.PropertyName, StringComparison.CurrentCulture)),
                    select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Take, select.Skip);
            }

            throw new NotSupportedException();
        }

        public Expression BuildWhereFindByIDPredicate(DbExpression expression, object[] keyData, params string[] keyNames)
        {
            throw new NotImplementedException();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression.IsNull())
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                Type collectionType = typeof(SubSonicTableTypeCollection<>).MakeGenericType(expression.Type);

                return (IQueryable)Activator.CreateInstance(collectionType,
                    tableTypeName, this, expression);
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression.IsNull())
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                return new SubSonicTableTypeCollection<TElement>(tableTypeName, this, expression);
            }
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }

        public async Task<object> ExecuteAsync(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }

        public object Execute(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            throw Error.NotImplemented();
        }

        public IDbPagedQuery ToPagedQuery(Expression expression, int size = 20)
        {
            throw new NotImplementedException();
        }

        public IDbQuery ToQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IDbQuery BuildDbQuery<TEntity>(DbQueryType queryType, IEnumerable<IEntityProxy> proxies)
        {
            throw new NotImplementedException();
        }
    }
}
