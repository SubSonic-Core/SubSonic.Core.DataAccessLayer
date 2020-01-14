using System.Collections.Generic;

namespace SubSonic.Interfaces
{
    public interface IDbPagedQuery
        : IDbQuery
    {
        /// <summary>
        /// Get record count of records matching the query criteria
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Get the page size of the returned dataset
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// get the number of pages that make up the dataset
        /// </summary>
        /// <remarks>
        /// PageCount = Ceiling(Count / PageSize)
        /// </remarks>
        int PageCount { get; }
        /// <summary>
        /// Get the data for the specified page number
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="number"></param>
        /// <returns></returns>
        IEnumerable<TEntity> GetRecordsForPage<TEntity>(int number);
    }
}
