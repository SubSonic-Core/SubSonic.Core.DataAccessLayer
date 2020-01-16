using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    public interface IDbPageCollection<out TEntity>
        : IEnumerable<TEntity>
    {
        int PageSize { get; }
        int PageNumber { get; set; }
        int PageCount { get; }
        int RecordCount { get; set; }

        IDbPagesCollection<TEntity> GetPages();

        /// <summary>
        /// Get the data for the specified page number
        /// </summary>
        /// <param name="number">page number to retrieve</param>
        /// <returns></returns>
        IEnumerable<TEntity> GetRecordsForPage(int number);
    }
}
