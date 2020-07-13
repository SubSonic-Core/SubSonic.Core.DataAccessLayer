using System.Collections;
using System.Collections.Generic;

namespace SubSonic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Not a true collection")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Not a true collection")]
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

        IDbPageCollection<TEntity> ToPagedCollection<TEntity>();
    }
}
