using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    using Schema;

    public interface ISqlGenerator
    {
        /// <summary>
        /// SqlFragment. Field values may change depending on the inheriting Generator.
        /// </summary>
        ISqlFragment sqlFragment { get; }

        /// <summary>
        /// Generates the command line.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GenerateCommandLine();

        /// <summary>
        /// Generates the constraints.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GenerateConstraints();

        /// <summary>
        /// Generates from list.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GenerateFromList();

        /// <summary>
        /// Generates the order by.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GenerateOrderBy();

        /// <summary>
        /// Generates the group by.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GenerateGroupBy();

        /// <summary>
        /// Generates the joins.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GenerateJoins();

        /// <summary>
        /// Gets the paging SQL wrapper.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator GetPagingSqlWrapper();

        /// <summary>
        /// Gets the select columns.
        /// </summary>
        /// <returns></returns>
        List<string> GetSelectColumns();

        /// <summary>
        /// Finds the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        IDbEntityProperty FindColumn(string columnName);

        /// <summary>
        /// Builds the select statement.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator BuildSelectStatement();

        /// <summary>
        /// Builds the paged select statement.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator BuildPagedSelectStatement();

        /// <summary>
        /// Builds the update statement.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator BuildUpdateStatement();

        /// <summary>
        /// Builds the insert statement.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator BuildInsertStatement();

        /// <summary>
        /// Builds the delete statement.
        /// </summary>
        /// <returns></returns>
        ISqlGenerator BuildDeleteStatement();
    }
}
