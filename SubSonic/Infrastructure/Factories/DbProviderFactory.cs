#if X86
// pretty sure the x86 SDK does not have this class
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
