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

        void WriteSqlSegment(string segment, bool debug = false);
    }
}
