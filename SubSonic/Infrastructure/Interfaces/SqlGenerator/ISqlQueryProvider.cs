using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public interface ISqlQueryProvider
        : ISqlGenerator
    {
        string ClientName { get; }

        void WriteSqlSegment(string segment, bool debug = false);
    }
}
