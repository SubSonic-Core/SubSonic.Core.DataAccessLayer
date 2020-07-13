using SubSonic.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace SubSonic
{
    using Factory;

    public abstract class DbProviderBuilder<TColumn>
    {
        protected ISqlQueryProvider Provider { get; }

        protected DbProviderBuilder(ISqlQueryProvider provider)
        {
            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        /// TODO: Re-Visit when this can be done based on configured db factory
        protected string GenerateDataType(DbType dbType, PropertyInfo info)
        {
            return Provider.GenerateColumnDataDefinition(dbType, info);
        }

        protected string GenerateDefault(IDbEntityProperty column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            return GenerateDefault(null, column.DbType);
        }

        protected string GenerateDefault(PropertyInfo info, DbType dbType)
        {
            string result = "";

            if (info is null)
            {
                result = $" {Provider.GenerateDefaultConstraint(dbType)}";
            }

            return result;
        }

        protected SubSonicDbProvider DbProvider
        {
            get
            {
                if (SubSonicContext.ServiceProvider.GetService<DbProviderFactory>() is SubSonicDbProvider client)
                {
                    return client;
                }

                throw new NotSupportedException();
            }
        }

        public abstract string GenerateSelectSql(string name, IEnumerable<TColumn> columns);

        public abstract IEnumerable<TColumn> GetColumnInformation();
    }
}
