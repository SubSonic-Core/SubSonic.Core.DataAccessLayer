using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;
    using SqlGenerator;

    public class SqlQueryProvider
        : ISqlQueryProvider
    {
        private const string PAGING_SQL =
@"
DECLARE @Page int;
DECLARE @PageSize int;

SELECT @Page = {4}, @PageSize = {5};

SET NOCOUNT ON;

-- create a temp table to hold order ids
DECLARE @TempTable TABLE (IndexId int identity, _keyID {6});

-- insert the table ids and row numbers into the memory table
INSERT INTO @TempTable
(
	_keyID
)
SELECT 
	{0}
	{1}
	{2}
-- select only those rows belonging to the proper page
	{3}
INNER JOIN @TempTable t ON {0} = t._keyID
WHERE t.IndexId BETWEEN ((@Page - 1) * @PageSize + 1) AND (@Page * @PageSize);";

        public static ISqlContext CreateSqlContext<TSqlFragment, TSqlMethods>()
            where TSqlFragment : class, ISqlFragment, new()
            where TSqlMethods : class, ISqlMethods, new()
        {
            return new SqlContext<TSqlFragment, TSqlMethods>();
        }

        protected SqlQueryProvider(ISqlContext sqlContext)
        {
            this.Context = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
        }

        public IDbEntityModel EntityModel { get; set; }

        public virtual string ClientName => string.Empty;

        public ISqlContext Context { get; }

        public void WriteSqlSegment(string segment, bool debug = false)
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildDeleteStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildInsertStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildPagedSelectStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildSelectStatement()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator BuildUpdateStatement()
        {
            throw new NotImplementedException();
        }

        public virtual IDbEntityProperty FindColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateCommandLine()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateConstraints()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateFromList()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateGroupBy()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateJoins()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlGenerator GenerateOrderBy()
        {
            throw new NotImplementedException();
        }

        public virtual string GetPagingSqlWrapper()
        {
            return PAGING_SQL;
        }

        public virtual List<string> GetSelectColumns()
        {
            throw new NotImplementedException();
        }
    }
}
