using SubSonic.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SubSonic
{
    public interface ISubSonicDbSetCollection<TEntity>
        : ISubSonicCollection<TEntity>
    {
        bool Delete(TEntity entity);
        IQueryable<TEntity> FindByID(object[] keyData, params string[] keyNames);
        TEntity FindByID(params object[] keyData);
    }
    
    public interface ISubSonicCollection<TEntity>
        : ISubSonicCollection, IOrderedQueryable<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IAsyncSubSonicQueryable<TEntity>, IQueryable, IEnumerable, ICollection<TEntity>
    {
        void AddRange(IEnumerable<TEntity> entities);
    }

    public interface ISubSonicDbSetCollection
    {
        void Add(object entity);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Implemented on generic sub interface")]
    public interface ISubSonicCollection
        : IQueryable, IEnumerable
    {
        IQueryable Load();
    }
}
