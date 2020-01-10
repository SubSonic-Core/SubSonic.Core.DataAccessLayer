using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure.Factory
{
    /// <summary>
    /// This is a Db Factory Provider Wrapper
    /// </summary>
    /// <typeparam name="TDbFactory"></typeparam>
    /// <remarks>
    /// Wanted the ability to wrap the existing DbProviderFactories which are sealed classes.
    /// We need to add some additional functionality to truly have a DAL that knows nothing about a db client.
    /// </remarks>
    public abstract class SubSonicDbProvider<TDbFactory>
        : SubSonicDbProvider
        where TDbFactory : DbProviderFactory
    {
        protected SubSonicDbProvider(TDbFactory factory)
            : base(factory) { }

        protected new TDbFactory Provider
        {
            get
            {
                if (base.Provider is TDbFactory factory)
                {
                    return factory;
                }

                return null;
            }
        }
    }

    public abstract class SubSonicDbProvider
        : DbProviderFactory
        
    {
        protected SubSonicDbProvider(DbProviderFactory factory)
        {
            Provider = factory ?? throw new ArgumentNullException(nameof(factory));
            InvariantName = factory.GetType().Namespace;
        }

        protected DbProviderFactory Provider { get; }

        public abstract ISqlQueryProvider QueryProvider { get; }

        public string InvariantName { get; }

        public override bool CanCreateDataSourceEnumerator => Provider.CanCreateDataSourceEnumerator;

        public override DbConnection CreateConnection() => Provider.CreateConnection();

        public override DbCommand CreateCommand() => Provider.CreateCommand();

        public override DbCommandBuilder CreateCommandBuilder() => Provider.CreateCommandBuilder();

        public override DbDataAdapter CreateDataAdapter() => Provider.CreateDataAdapter();

        public override DbParameter CreateParameter() => Provider.CreateParameter();

        public abstract DbParameter CreateParameter(string name, object value);

        public abstract DbParameter CreateParameter(SubSonicParameter parameter);

        public abstract DbParameter CreateSubSonicParameter(string name, object value, IDbEntityProperty entityProperty);

        public abstract DbParameter CreateStoredProcedureParameter(string name, object value, bool mandatory, int size, bool isUserDefinedTableParameter, string udtType, ParameterDirection direction);

        public override DbConnectionStringBuilder CreateConnectionStringBuilder() => Provider.CreateConnectionStringBuilder();

        public override DbDataSourceEnumerator CreateDataSourceEnumerator() => Provider.CreateDataSourceEnumerator();

        public override bool Equals(object obj) => Provider.Equals(obj);

        public override int GetHashCode() => Provider.GetHashCode();

        public override string ToString() => Provider.ToString();

        //public abstract DbType GetDbType(Type netType, bool unicode = false);
    }
}
