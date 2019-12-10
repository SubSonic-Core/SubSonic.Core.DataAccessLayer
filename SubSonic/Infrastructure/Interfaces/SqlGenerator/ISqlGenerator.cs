using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    using Schema;

    public interface ISqlGenerator
    {
        string ClientName { get; set; }

        //SqlQuery Query { get; set; }

        /// <summary>
        /// SqlFragment. Field values may change depending on the inheriting Generator.
        /// </summary>
        ISqlFragment sqlFragment { get; }

        /// <summary>
        /// Generates the command line.
        /// </summary>
        /// <returns></returns>
        string GenerateCommandLine();

        /// <summary>
        /// Generates the constraints.
        /// </summary>
        /// <returns></returns>
        string GenerateConstraints();

        /// <summary>
        /// Generates from list.
        /// </summary>
        /// <returns></returns>
        string GenerateFromList();

        /// <summary>
        /// Generates the order by.
        /// </summary>
        /// <returns></returns>
        string GenerateOrderBy();

        /// <summary>
        /// Generates the group by.
        /// </summary>
        /// <returns></returns>
        string GenerateGroupBy();

        /// <summary>
        /// Generates the joins.
        /// </summary>
        /// <returns></returns>
        string GenerateJoins();

        /// <summary>
        /// Gets the paging SQL wrapper.
        /// </summary>
        /// <returns></returns>
        string GetPagingSqlWrapper();

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
        string BuildSelectStatement();

        /// <summary>
        /// Builds the paged select statement.
        /// </summary>
        /// <returns></returns>
        string BuildPagedSelectStatement();

        /// <summary>
        /// Builds the update statement.
        /// </summary>
        /// <returns></returns>
        string BuildUpdateStatement();

        /// <summary>
        /// Builds the insert statement.
        /// </summary>
        /// <returns></returns>
        string BuildInsertStatement();

        /// <summary>
        /// Builds the delete statement.
        /// </summary>
        /// <returns></returns>
        string BuildDeleteStatement();

        /// <summary>
        /// Sets the insert query.
        /// </summary>
        /// <param name="q">The q.</param>
        //void SetInsertQuery(Insert q);
    }
}
