using SubSonic.Infrastructure.Schema;
using SubSonic.Infrastructure.SqlGenerator;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface ISqlQueryProvider
        : ISqlGenerator
    {
        string ClientName { get; }

        IDbEntityModel EntityModel { get; set; }

        string GenerateSqlFor(Expression query);
    }
}
