using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public class AnsiSqlGenerator
        : ISqlGenerator
    {
        public AnsiSqlGenerator(ISqlContext sqlContext)
        {
            SqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
        }

        public ISqlContext SqlContext { get; }

        public ISqlGenerator BuildDeleteStatement()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator BuildInsertStatement()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator BuildPagedSelectStatement()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator BuildSelectStatement()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator BuildUpdateStatement()
        {
            throw new NotImplementedException();
        }

        public IDbEntityProperty FindColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator GenerateCommandLine()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator GenerateConstraints()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator GenerateFromList()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator GenerateGroupBy()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator GenerateJoins()
        {
            throw new NotImplementedException();
        }

        public ISqlGenerator GenerateOrderBy()
        {
            throw new NotImplementedException();
        }

        public string GetPagingSqlWrapper()
        {
            throw new NotImplementedException();
        }

        public List<string> GetSelectColumns()
        {
            throw new NotImplementedException();
        }
    }
}
