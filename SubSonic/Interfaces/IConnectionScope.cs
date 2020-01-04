using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic
{
    public interface IConnectionScope
    {
        DbConnection Connection { get; }
    }
}
