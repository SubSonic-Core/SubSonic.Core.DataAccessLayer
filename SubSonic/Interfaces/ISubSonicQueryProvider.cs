using SubSonic.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface ISubSonicQueryProvider<out TEntity>
        : IDbSubSonicQueryProvider
    {
        
    }
}
