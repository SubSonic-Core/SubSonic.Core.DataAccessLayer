using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic
{
    public interface ISubSonicCollection<out TEntity>
        : ISubSonicCollection, IQueryable<TEntity>, IEnumerable<TEntity>
    {


    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Implemented on generic sub interface")]
    public interface ISubSonicCollection
        : IQueryable, IEnumerable
    {
    }
}
