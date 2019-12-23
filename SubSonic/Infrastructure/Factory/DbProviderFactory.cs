using System;
using System.Collections.Generic;
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
    public class DbProviderFactory<TDbFactory>
        : DbProviderFactory
        where TDbFactory : DbProviderFactory
    {
        protected const string InstanceFieldName = "Instance";

        protected DbProviderFactory()
        {
            Provider = (TDbFactory)typeof(TDbFactory).GetField(InstanceFieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).GetValue(null);

            InvariantName = Provider.GetType().Namespace;
        }

        public string InvariantName { get; }

        public override bool CanCreateDataSourceEnumerator => Provider.CanCreateDataSourceEnumerator;

        protected TDbFactory Provider { get; }

        public override DbConnection CreateConnection() => Provider.CreateConnection();

        public override DbCommand CreateCommand() => Provider.CreateCommand();

        public override DbCommandBuilder CreateCommandBuilder() => Provider.CreateCommandBuilder();

        public override DbDataAdapter CreateDataAdapter() => Provider.CreateDataAdapter();

        public override DbParameter CreateParameter() => Provider.CreateParameter();

        public override DbConnectionStringBuilder CreateConnectionStringBuilder() => Provider.CreateConnectionStringBuilder();

        public override DbDataSourceEnumerator CreateDataSourceEnumerator() => Provider.CreateDataSourceEnumerator();

        public override bool Equals(object obj) => Provider.Equals(obj);

        public override int GetHashCode() => Provider.GetHashCode();

        public override string ToString() => Provider.ToString();

        public virtual int GetDbType(Type netType)
        {
            return (int)netType.GetDbType();
        }
    }
}
