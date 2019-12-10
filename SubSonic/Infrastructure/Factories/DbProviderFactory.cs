#if DB_PROVIDER_NOT_DEFINED
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SubSonic.Infrastructure.Factories
{
    public class DbProviderFactory
    {
        protected DbProviderFactory()
        {

        }

        public virtual bool CanCreateDataSourceEnumerator { get => false; }

        public virtual DbConnection CreateConnection()
        {
            throw new NotImplementedException();
        }
        public virtual DbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }
        public virtual DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotImplementedException();
        }
        public virtual DbDataAdapter CreateDataAdapter()
        {
            throw new NotImplementedException();
        }
        public virtual DbParameter CreateParameter()
        {
            throw new NotImplementedException();
        }
        public virtual DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
