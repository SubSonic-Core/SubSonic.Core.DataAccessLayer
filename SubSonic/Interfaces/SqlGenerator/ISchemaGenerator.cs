using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public interface ISchemaGenerator
    {
        /// <summary>
        /// Builds a CREATE TABLE statement.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        string BuildCreateTableStatement(IDbEntityModel entityModel);

        /// <summary>
        /// Builds a DROP TABLE statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        string BuildDropTableStatement(string tableName);

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="column">The column.</param>
        string BuildAddColumnStatement(string tableName, IDbEntityProperty entityProperty);

        /// <summary>
        /// Alters the column.
        /// </summary>
        /// <param name="column">The column.</param>
        string BuildAlterColumnStatement(IDbEntityProperty entityProperty);

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        string BuildDropColumnStatement(string tableName, string columnName);

        /// <summary>
        /// Gets the type of the native.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        string GetNativeType(DbType dbType);

        /// <summary>
        /// Generates the columns.
        /// </summary>
        /// <param name="table">Table containing the columns.</param>
        /// <returns>
        /// SQL fragment representing the supplied columns.
        /// </returns>
        string GenerateColumns(IDbEntityModel entityModel);

        /// <summary>
        /// Sets the column attributes.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        string GenerateColumnAttributes(IDbEntityProperty entityProperty);

        IDbEntityModel GetEntityModelFromDbContext(SubSonicContext dbContext, string tableName);

        string[] GetTableList(SubSonicContext dbContext);

        DbType GetDbType(string sqlType);

        string ClientName { get; set; }
    }
}
