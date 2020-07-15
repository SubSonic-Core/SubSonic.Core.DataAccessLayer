using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SubSonic
{
    public interface ISubSonicSetCollection<TEntity>
        : ISubSonicCollection<TEntity>
        , ISubSonicSetCollection
    {
        bool Delete(TEntity entity);
        IQueryable<TEntity> FindByID(object[] keyData, params string[] keyNames);
        TEntity FindByID(params object[] keyData);
    }
    
    public interface ISubSonicCollection<TEntity>
        : ISubSonicCollection, IAsyncSubSonicQueryable<TEntity>, IOrderedQueryable<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable, ICollection<TEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        new int Count { get; }

        void AddRange(IEnumerable<TEntity> entities);
    }

    public interface ISubSonicSetCollection
    {
        void Add(object entity);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Implemented on generic sub interface")]
    public interface ISubSonicCollection
        : IQueryable, IEnumerable
    {
        int Count { get; }
        IEnumerable ToArray();
        IQueryable Load();
    }
}
