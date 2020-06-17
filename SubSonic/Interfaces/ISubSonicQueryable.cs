using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic
{
    public interface ISubSonicCollection<TEntity>
        : ISubSonicCollection, IOrderedQueryable<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable, ICollection<TEntity>
    {
        IQueryable<TEntity> FindByID(object[] keyData, params string[] keyNames);
        IQueryable<TEntity> FindByID(params object[] keyData);
        IQueryable<TEntity> Load();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Implemented on generic sub interface")]
    public interface ISubSonicCollection
        : IQueryable, IEnumerable
    {
    }
}
