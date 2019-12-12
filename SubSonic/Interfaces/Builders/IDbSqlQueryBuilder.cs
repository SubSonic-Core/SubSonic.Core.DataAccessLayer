using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure.Builders
{
    public interface IDbSqlQueryBuilder
    {
        IDbSqlQueryBuilder BuildSqlQuery(SqlQueryType sqlQueryType, ISqlQueryProvider sqlQueryProvider);

        IDbSqlQueryBuilder BuildSqlQuery(Expression expression);

        IDbSqlQueryBuilder BuildCreateQuery();

        object ToQueryObject();
    }
}
