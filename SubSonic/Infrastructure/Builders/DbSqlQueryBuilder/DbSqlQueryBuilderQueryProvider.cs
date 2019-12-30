using System;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure.Builders
{
    using Logging;
    using SubSonic.Linq.Expressions;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    public partial class DbSqlQueryBuilder
    {
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
            //DbContext.Cache.Flush();

            using (SharedDbConnectionScope Scope = DbContext.ServiceProvider.GetService<SharedDbConnectionScope>())
            {
                try
                {
                    Scope.CurrentConnection.Open();

                    CmdBehavior = typeof(TResult).IsEnumerable() ? CommandBehavior.Default : CommandBehavior.SingleRow;
                    
                    DbDataReader reader = (DbDataReader)Execute(expression);

                    Type elementType = typeof(TResult).GetQualifiedType();

                    while (reader.Read())
                    {
                        DbContext.Cache.Add(elementType, reader.ActivateAndLoadInstanceOf(elementType));
                    }

                    return DbContext.Cache.Where<TResult>(elementType, this, expression);
                }
                finally
                {
                    Scope.CurrentConnection.Close();
                }
            }
        }

        public object Execute(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            using (AutomaticConnectionScope Scope = DbContext.ServiceProvider.GetService<AutomaticConnectionScope>())
            using (DbCommand cmd = GetCommand(Scope.Connection, expression))
            using (var perf = logger.Start(GetType(), $"{nameof(Execute)}::{((DbSelectExpression)expression).From.Type.GetTypeName()}"))
            {
                    return cmd.ExecuteReader(CmdBehavior);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Builder paramatizes all user inputs when sql expression tree is visited")]
        protected DbCommand GetCommand(DbConnection connection, Expression expression)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            IDbQueryObject query = ToQueryObject(expression);

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            foreach(SubSonicParameter parameter in query.Parameters)
            {
                DbParameter dbParameter = command.CreateParameter();

                dbParameter.Map(parameter);

                command.Parameters.Add(dbParameter);
            }
            
            command.CommandText = query.Sql;

            return command;
        }
    }
}
