using SubSonic.Infrastructure.Schema;
using SubSonic.Infrastructure.SqlGenerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface ISqlQueryProvider
        : ISqlGenerator
    {
        string ClientName { get; }

        IDbEntityModel EntityModel { get; set; }

        void WriteSqlSegment(string segment, bool debug = false);
    }
}
