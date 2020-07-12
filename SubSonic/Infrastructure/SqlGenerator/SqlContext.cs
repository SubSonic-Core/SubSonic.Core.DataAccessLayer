using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;


namespace SubSonic.Infrastructure.SqlGenerator
{
    using Factory;

    internal class SqlContext<TSqlFragment, TSqlMethods>
        : ISqlContext
        where TSqlFragment : class, ISqlFragment, new()
        where TSqlMethods : class, ISqlMethods, new()
    {
        private ISqlFragment sqlFragment;
        private ISqlMethods sqlMethods;
        ISqlFragment ISqlContext.Fragments => sqlFragment ?? (sqlFragment = new TSqlFragment());

        ISqlMethods ISqlContext.Methods => sqlMethods ?? (sqlMethods = new TSqlMethods());

        public SubSonicDbProvider Provider
        {
            get
            {
                DbProviderFactory factory = SubSonicContext.ServiceProvider.GetService<DbProviderFactory>();

                if (factory is SubSonicDbProvider provider)
                {
                    return provider;
                }

                throw new NotSupportedException();
            }
        }

        public SqlContext()
        {
        }
    
    }
}
