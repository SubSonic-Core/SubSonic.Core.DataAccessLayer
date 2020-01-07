using SubSonic.Infrastructure.Schema;
using SubSonic.Infrastructure.SqlGenerator;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Infrastructure
{
    public interface ISqlQueryProvider
        : ISqlGenerator
    {
        string ClientName { get; }

        IDbEntityModel EntityModel { get; set; }

        string GenerateSqlFor(Expression query);
        string GenerateColumnDataDefinition(DbType dbType, PropertyInfo info);
        string GenerateDefaultConstraint(DbType dbType);
    }
}
