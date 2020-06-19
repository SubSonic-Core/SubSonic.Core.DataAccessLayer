using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace SubSonic.Infrastructure.Builders
{
    using Linq;
    using Linq.Expressions;
    using Schema;
    using Logging;
    using SysLinq = System.Linq;

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

        public Expression BuildCall(string nameOfCallee, Expression collection, Expression lambda)
        {
            throw new NotImplementedException();
        }

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

        public Expression BuildLogicalIn(Expression body, PropertyInfo property, SysLinq.IQueryable queryable, DbGroupOperator group)
        {
            throw new NotImplementedException();
        }

        public Expression BuildLogicalIn(Expression body, PropertyInfo property, IEnumerable<Expression> values, DbGroupOperator group)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(SysLinq.IQueryable queryable)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(SysLinq.IQueryable queryable, Expression eWhere)
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

        public Expression BuildSelect(Expression eSelect, int count)
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
                    typeof(SysLinq.IQueryable<>).MakeGenericType(property.PropertyType),
                    select.From,
                    select.Columns.Where(x => x.PropertyName.Equals(property.PropertyName, StringComparison.CurrentCulture)),
                    select.Where, select.OrderBy, select.GroupBy, select.IsDistinct, select.Take);
            }

            throw new NotSupportedException();
        }

        public Expression BuildSelect(Expression eSelect, IEnumerable<DbOrderByDeclaration> orderBy)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(Expression eSelect, IEnumerable<Expression> groupBy)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect(Expression eSelect, DbExpressionType eType, IEnumerable<Expression> expressions)
        {
            throw new NotImplementedException();
        }

        public Expression BuildSelect<TEntity, TColumn>(Expression eSelect, Expression<Func<TEntity, TColumn>> selector)
        {
            throw new NotImplementedException();
        }

        public Expression BuildWhere(DbTableExpression table, Expression where, Type type, LambdaExpression predicate)
        {
            throw new NotImplementedException();
        }

        public Expression BuildWhereExists<TEntity>(DbTableExpression dbTableExpression, Type type, Expression<Func<TEntity, SysLinq.IQueryable>> query)
        {
            throw new NotImplementedException();
        }

        public Expression BuildWhereNotExists<TEntity>(DbTableExpression from, Type type, Expression<Func<TEntity, SysLinq.IQueryable>> query)
        {
            throw new NotImplementedException();
        }

        public Expression BuildWherePredicate(Expression collection, Expression logical)
        {
            throw new NotImplementedException();
        }

        public SysLinq.IQueryable CreateQuery(Expression expression)
        {
            if (expression.IsNull())
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (IPerformanceLogger performance = logger.Start(GetType(), nameof(CreateQuery)))
            {
                Type collectionType = typeof(SubSonicTableTypeCollection<>).MakeGenericType(expression.Type);

                return (SysLinq.IQueryable)Activator.CreateInstance(collectionType,
                    tableTypeName, this, expression);
            }
        }

        public SysLinq.IQueryable<TElement> CreateQuery<TElement>(Expression expression)
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

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IDbPagedQuery ToPagedQuery(Expression expression, int size = 20)
        {
            throw new NotImplementedException();
        }

        public IDbQuery ToQuery(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
