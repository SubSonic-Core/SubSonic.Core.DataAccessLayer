namespace SubSonic.SqlGenerator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public interface ISqlFragment
    {
        string NOT { get; }
        string AND { get; }
        string AS { get; }
        string ASC { get; }
        string BETWEEN { get; }
        string NOT_BETWEEN { get; }
        string CROSS_JOIN { get; }
        string DELETE_FROM { get; }
        string DESC { get; }
        string DISTINCT { get; }
        string EQUAL_TO { get; }
        string FROM { get; }
        string GROUP_BY { get; }
        string HAVING { get; }
        string IN { get; }

        string INNER_JOIN { get; }

        string INSERT_INTO { get; }
        string JOIN_PREFIX { get; }
        string LEFT_INNER_JOIN { get; }
        string LEFT_JOIN { get; }
        string LEFT_OUTER_JOIN { get; }
        string NOT_EQUAL_TO { get; }
        string NOT_IN { get; }
        string ON { get; }
        string OR { get; }
        string ORDER_BY { get; }
        string OUTER_JOIN { get; }
        string RIGHT_INNER_JOIN { get; }
        string RIGHT_JOIN { get; }
        string RIGHT_OUTER_JOIN { get; }
        string SELECT { get; }
        string SET { get; }
        string SPACE { get; }
        string TOP { get; }
        string UNEQUAL_JOIN { get; }
        string UPDATE { get; }
        string WHERE { get; }
        string IS_NULL { get; }
        string IS_NOT_NULL { get; }
        string ROW_NUMBER { get; }
        string NULL { get; }
        string COMMA { get; }
        string LEFT_PARENTHESIS { get; }
        string RIGHT_PARENTHESIS { get; }
        string QOUTE { get; }
        string COUNT { get; }
        string COUNTBIG { get; }
        string MIN { get; }
        string AVG { get; }
        string MAX { get; }
        string SUM { get; }
        string CROSS_APPLY { get; }
        string OUTER_APPLY { get; }
        string ASTRIX { get; }
        string EXISTS { get; }
        string WITH { get; }
        string ROWS { get; }
        string OFFSET { get; }
        string OPTION { get; }
        string RECOMPILE { get; }
        string FETCH { get; }
        string NEXT { get; }
        string ONLY { get; }
        string OUTPUT_INSERTED_INTO { get; }
        string VALUES { get; }
        string STATEMENT_END { get; }
        string DECLARE { get; }
    }
}
