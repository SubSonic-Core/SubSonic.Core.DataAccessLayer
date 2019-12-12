using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public class AnsiSqlFragment
        : ISqlFragment
    {
        public string AND { get; set; } = "AND";
        public string AS { get; set; } = "AS";
        public string ASC { get; set; } = "ASC";
        public string BETWEEN { get; set; } = "BETWEEN";
        public string NOT_BETWEEN { get; set; } = "NOT BETWEEN";
        public string CROSS_JOIN { get; set; } = "CROSS JOIN";
        public string DELETE_FROM { get; set; } = "DELETE FROM";
        public string DESC { get; set; } = "DESC";
        public string DISTINCT { get; set; } = "DISTINCT";
        public string EQUAL_TO { get; set; } = "=";
        public string FROM { get; set; } = "FROM";
        public string GROUP_BY { get; set; } = "GROUP BY";
        public string HAVING { get; set; } = "HAVING";
        public string IN { get; set; } = "IN";
        public string INNER_JOIN { get; set; } = "INNER JOIN";
        public string INSERT_INTO { get; set; } = "INSERT INTO";
        public string JOIN_PREFIX { get; set; } = "J";
        public string LEFT_INNER_JOIN { get; set; } = "LEFT INNER JOIN";
        public string LEFT_JOIN { get; set; } = "LEFT JOIN";
        public string LEFT_OUTER_JOIN { get; set; } = "LEFT OUTER JOIN";
        public string NOT_EQUAL_TO { get; set; } = "<>";
        public string NOT_IN { get; set; } = "NOT IN";
        public string ON { get; set; } = "ON";
        public string NULL { get; set; } = "NULL";
        public string IS_NULL { get; set; } = $"IS NULL";
        public string IS_NOT_NULL { get; set; } = "IS NOT NULL";
        public string OR { get; set; } = "OR";
        public string ORDER_BY { get; set; } = "ORDER BY";
        public string OUTER_JOIN { get; set; } = "OUTER JOIN";
        public string RIGHT_INNER_JOIN { get; set; } = "RIGHT INNER JOIN";
        public string RIGHT_JOIN { get; set; } = "RIGHT JOIN";
        public string RIGHT_OUTER_JOIN { get; set; } = "RIGHT OUTER JOIN";
        public string SELECT { get; set; } = "SELECT";
        public string SET { get; set; } = "SET";
        public string SPACE { get; set; } = " ";
        public string TOP { get; set; } = "TOP";
        public string UNEQUAL_JOIN { get; set; } = "JOIN";
        public string UPDATE { get; set; } = "UPDATE";
        public string WHERE { get; set; } = "WHERE";
        public string ROW_NUMBER { get; set; } = "ROW_NUMBER() OVER";
        public string COMMA { get; set; } = ",";
        public string LEFT_PARENTHESIS { get; set; } = "(";
        public string RIGHT_PARENTHESIS { get; set; } = ")";
        public string QOUTE { get; set; } = "'";
        public string COUNT { get; set; } = "COUNT";
        public string MIN { get; set; } = "MIN";
        public string MAX { get; set; } = "MAX";
        public string AVG { get; set; } = "AVG";
        public string SUM { get; set; } = "SUM";
    }
}
