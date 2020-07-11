using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public class AnsiSqlFragment
        : ISqlFragment
    {
        public virtual string NOT => "NOT";
        public virtual string AND => "AND";
        public virtual string AS => "AS";
        public virtual string ASC => "ASC";
        public virtual string BETWEEN => "BETWEEN";
        public virtual string NOT_BETWEEN => "NOT BETWEEN";
        public virtual string CROSS_JOIN => "CROSS JOIN";
        public virtual string DELETE_FROM => "DELETE FROM";
        public virtual string DELETED => "DELETED";
        public virtual string DESC => "DESC";
        public virtual string DISTINCT => "DISTINCT";
        public virtual string EQUAL_TO => "=";
        public virtual string EXISTS => "EXISTS";
        public virtual string FROM => "FROM";
        public virtual string GROUP_BY => "GROUP BY";
        public virtual string HAVING => "HAVING";
        public virtual string IN => "IN";
        public virtual string CROSS_APPLY => "CROSS APPLY";
        public virtual string OUTER_APPLY => "OUTER APPLY";
        public virtual string INNER_JOIN => "INNER JOIN";
        public virtual string INTO => "INTO";
        public virtual string INSERT_INTO => $"INSERT {INTO}";
        public virtual string INSERTED => "INSERTED";
        public virtual string JOIN_PREFIX => "J";
        public virtual string LEFT_INNER_JOIN => "LEFT INNER JOIN";
        public virtual string LEFT_JOIN => "LEFT JOIN";
        public virtual string LEFT_OUTER_JOIN => "LEFT OUTER JOIN";
        public virtual string NOT_EQUAL_TO => "<>";
        public virtual string NOT_IN => $"{NOT} IN";
        public virtual string ON => "ON";
        public virtual string NULL => "NULL";
        public virtual string IS_NULL => $"IS {NULL}";
        public virtual string IS_NOT_NULL => $"IS {NOT} {NULL}";
        public virtual string OR => "OR";
        public virtual string ORDER_BY => "ORDER BY";
        public virtual string OUTER_JOIN => "OUTER JOIN";
        public virtual string RIGHT_INNER_JOIN => "RIGHT INNER JOIN";
        public virtual string RIGHT_JOIN => "RIGHT JOIN";
        public virtual string RIGHT_OUTER_JOIN => "RIGHT OUTER JOIN";
        public virtual string SELECT => "SELECT";
        public virtual string SET => "SET";
        public virtual string SPACE => " ";
        public virtual string TOP => "TOP";
        public virtual string UNEQUAL_JOIN => "JOIN";
        public virtual string UPDATE => "UPDATE";
        public virtual string WHERE => "WHERE";
        public virtual string WITH => "WITH";
        public virtual string ROW_NUMBER => "ROW_NUMBER() OVER";
        public virtual string ROWS => "ROWS";
        public virtual string COMMA => ",";
        public virtual string LEFT_PARENTHESIS => "(";
        public virtual string RIGHT_PARENTHESIS => ")";
        public virtual string QOUTE => "'";
        public virtual string COUNT => "COUNT";
        public virtual string COUNTBIG => "COUNT_BIG";
        public virtual string MIN => "MIN";
        public virtual string MAX => "MAX";
        public virtual string AVG => "AVG";
        public virtual string SUM => "SUM";
        public virtual string ASTRIX => "*";
        public virtual string OFFSET => "OFFSET";
        public virtual string FETCH => "FETCH";
        public virtual string NEXT => "NEXT";
        public virtual string OPTION => "OPTION";
        public virtual string ONLY => "ONLY";
        public virtual string RECOMPILE => "RECOMPILE";
        public virtual string OUTPUT => "OUTPUT";
        public virtual string OUTPUT_INSERTED_INTO => $"{OUTPUT} {INSERTED}.{ASTRIX} {INTO}";
        public virtual string VALUES => "VALUES";
        public virtual string STATEMENT_END => ";";
        public virtual string DECLARE => "DECLARE";
    }
}

