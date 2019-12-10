using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    using SqlGenerator;
    using System.Reflection;

    public static class SqlQueryProviderFactory
    {
        private static Dictionary<string, ISqlQueryProvider> ProviderFactories = new Dictionary<string, ISqlQueryProvider>();

        private const string InstanceFieldName = "Instance";

        public static void RegisterFactory(string dBProviderInvariantName, Type sqlQueryProviderType)
        {
            if (string.IsNullOrEmpty(dBProviderInvariantName))
            {
                throw new ArgumentException("", nameof(dBProviderInvariantName));
            }

            if (sqlQueryProviderType is null)
            {
                throw new ArgumentNullException(nameof(sqlQueryProviderType));
            }

            FieldInfo fieldInstanceInfo = sqlQueryProviderType.GetField(InstanceFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);

            if (fieldInstanceInfo.IsNull())
            {
                throw new MissingFieldException(sqlQueryProviderType.FullName, InstanceFieldName);
            }

            Type interfaceType = typeof(ISqlQueryProvider);

            if (fieldInstanceInfo.FieldType.GetInterface(interfaceType.FullName).IsNull())
            {
                throw new NotImplementedException(SubSonicErrorMessages.MissingInterfaceImplementation.Format(sqlQueryProviderType.FullName, interfaceType.Name));
            }

            ISqlQueryProvider sqlQueryProvider = (ISqlQueryProvider)fieldInstanceInfo.GetValue(null);

            if(sqlQueryProvider.IsNull())
            {
                throw new NullReferenceException(SubSonicErrorMessages.SqlQueryProviderIsNull.Format(sqlQueryProviderType.Name));
            }

            if (!ProviderFactories.ContainsKey(dBProviderInvariantName))
            {
                ProviderFactories.Add(dBProviderInvariantName, sqlQueryProvider);
            }
        }

        public static ISqlQueryProvider GetProvider(string dBProviderInvariantName)
        {
            return ProviderFactories[dBProviderInvariantName];
        }

        public static bool TryGetProvider(string dBProviderInvariantName, out ISqlQueryProvider provider)
        {
            return ProviderFactories.TryGetValue(dBProviderInvariantName, out provider);
        }
    }
}
