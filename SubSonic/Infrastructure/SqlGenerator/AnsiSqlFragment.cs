using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public class AnsiSqlFragment
        : ISqlFragment
    {
        public string AND => "AND";
        public string AS => "AS";
        public string ASC => "ASC";
        public string BETWEEN => "BETWEEN";
        public string NOT_BETWEEN => "NOT BETWEEN";
        public string CROSS_JOIN => "CROSS JOIN";
        public string DELETE_FROM => "DELETE FROM";
        public string DESC => "DESC";
        public string DISTINCT => "DISTINCT";
        public string EQUAL_TO => "=";
        public string EXISTS => "EXISTS";
        public string FROM => "FROM";
        public string GROUP_BY => "GROUP BY";
        public string HAVING => "HAVING";
        public string IN => "IN";
        public string CROSS_APPLY => "CROSS APPLY";
        public string OUTER_APPLY => "OUTER APPLY";
        public string INNER_JOIN => "INNER JOIN";
        public string INSERT_INTO => "INSERT INTO";
        public string JOIN_PREFIX => "J";
        public string LEFT_INNER_JOIN => "LEFT INNER JOIN";
        public string LEFT_JOIN => "LEFT JOIN";
        public string LEFT_OUTER_JOIN => "LEFT OUTER JOIN";
        public string NOT_EQUAL_TO => "<>";
        public string NOT_IN => "NOT IN";
        public string ON => "ON";
        public string NULL => "NULL";
        public string IS_NULL => $"IS NULL";
        public string IS_NOT_NULL => "IS NOT NULL";
        public string OR => "OR";
        public string ORDER_BY => "ORDER BY";
        public string OUTER_JOIN => "OUTER JOIN";
        public string RIGHT_INNER_JOIN => "RIGHT INNER JOIN";
        public string RIGHT_JOIN => "RIGHT JOIN";
        public string RIGHT_OUTER_JOIN => "RIGHT OUTER JOIN";
        public string SELECT => "SELECT";
        public string SET => "SET";
        public string SPACE => " ";
        public string TOP => "TOP";
        public string UNEQUAL_JOIN => "JOIN";
        public string UPDATE => "UPDATE";
        public string WHERE => "WHERE";
        public string ROW_NUMBER => "ROW_NUMBER() OVER";
        public string COMMA => ",";
        public string LEFT_PARENTHESIS => "(";
        public string RIGHT_PARENTHESIS => ")";
        public string QOUTE => "'";
        public string COUNT => "COUNT";
        public string MIN => "MIN";
        public string MAX => "MAX";
        public string AVG => "AVG";
        public string SUM => "SUM";
        public string ASTRIX => "*";
    }
}

