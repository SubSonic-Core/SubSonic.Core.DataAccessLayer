using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace SubSonic.Linq.Expressions
{
    using Interfaces;
    using Infrastructure.Builders;
    using Structure;    

    public class DbWhereExpression
        : DbExpression
        , IArgumentProvider
        , IDbParameterProvider
    {
        protected internal DbWhereExpression(MethodInfo method, IEnumerable<Expression> arguments)
            : base(GetDbExpressionType(method), typeof(bool))
        {
            Method = method;
            Arguments = new ReadOnlyCollection<Expression>(arguments.ToList());
        }

        protected internal DbWhereExpression(MethodInfo method, IEnumerable<Expression> arguments, Expression expression, IEnumerable<DbParameter> parameters, bool canReadFromCache)
            : this(method, arguments)
        {
            Expression = expression;
            Parameters = new ReadOnlyCollection<DbParameter>(parameters.ToList());
            CanReadFromCache = canReadFromCache;
        }

        public MethodInfo Method { get; }

        public IReadOnlyCollection<Expression> Arguments { get; }

        public int ArgumentCount
        {
            get
            {
                return Arguments.Count;
            }
        }

        public IReadOnlyCollection<DbParameter> Parameters { get; }

        public bool CanReadFromCache { get; }

        public Expression Expression { get; }

        public Expression GetArgument(int index)
        {
            return Arguments.ElementAt(index);
        }

        public override bool CanReduce => true;

        private static DbExpressionType GetDbExpressionType(MethodInfo method)
        {
            if (method is null)
            {
                throw Error.ArgumentNull(nameof(method));
            }
            if (method.Name.EndsWith(nameof(DbExpressionType.NotExists), StringComparison.CurrentCulture))
            {
                return DbExpressionType.NotExists;
            }
            else if (method.Name.EndsWith(nameof(DbExpressionType.Exists), StringComparison.CurrentCulture))
            {
                return DbExpressionType.Exists;
            }
            else
            {
                return DbExpressionType.Where;
            }
        }

        public override Expression Reduce()
        {
            return Call(Method, Arguments);
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (visitor is DbExpressionVisitor db)
            {
                return db.VisitWhere(this);
            }

            return base.Accept(visitor);
        }
    }

    public partial class DbExpression
    {
        public static Expression DbWhere(MethodInfo method, IEnumerable<Expression> arguments)
        {
            return new DbWhereExpression(method, arguments);
        }

        public static Expression DbWhere(MethodInfo method, IEnumerable<Expression> arguments, Expression expression, IEnumerable<DbParameter> parameters, bool canReadFromCache)
        {
            return new DbWhereExpression(method, arguments, expression, parameters, canReadFromCache);
        }
    }
}
