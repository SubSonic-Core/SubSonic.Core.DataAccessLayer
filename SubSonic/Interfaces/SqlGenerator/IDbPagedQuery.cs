using System.Collections.Generic;

namespace SubSonic
{
    public interface IDbPagedQuery
        : IDbQuery
    {
        /// <summary>
        /// Get record count of records matching the query criteria
        /// </summary>
        int RecordCount { get; }
        /// <summary>
        /// Get the page size of the returned dataset
        /// </summary>
        int PageSize { get; }
        int PageNumber { get; set; }
        /// <summary>
        /// get the number of pages that make up the dataset
        /// </summary>
        /// <remarks>
        /// PageCount = Ceiling(Count / PageSize)
        /// </remarks>
        int PageCount { get; }

        IDbPagedCollection<TEntity> ToPagedCollection<TEntity>();
    }
}
