using System.Collections.Generic;

namespace SubSonic
{
    public interface IDbPageCollection<out TEntity>
        : IEnumerable<TEntity>
        , IAsyncEnumerable<TEntity>
    {
        int PageSize { get; }
        int PageNumber { get; set; }
        int PageCount { get; }
        int RecordCount { get; set; }
        /// <summary>
        /// Get the page collection with iterator
        /// </summary>
        /// <returns></returns>
        IDbPagesCollection<TEntity> GetPages();

        /// <summary>
        /// Get the data for the specified page number
        /// </summary>
        /// <param name="number">page number to retrieve</param>
        /// <returns></returns>
        IEnumerable<TEntity> GetRecordsForPage(int number);
        /// <summary>
        /// Get the entities for the current page;
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetRecordsForPage();
    }
}
