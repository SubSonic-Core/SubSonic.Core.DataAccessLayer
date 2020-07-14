#if NETSTANDARD2_0
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SubSonic
{
    public static class DbProviderFactories
    {
        private static readonly ConcurrentDictionary<string, DbProviderFactory> registeredFactories = new ConcurrentDictionary<string, DbProviderFactory>();
        private const string AssemblyQualifiedNameColumnName = "AssemblyQualifiedName";
        private const string InvariantNameColumnName = "InvariantName";
        private const string NameColumnName = "Name";
        private const string DescriptionColumnName = "Description";
        private const string ProviderGroupColumnName = "DbProviderFactories";
        private const string InstanceFieldName = "Instance";
        public static DbProviderFactory GetFactory(DataRow providerRow)
        {
            if (providerRow is null)
            {
                throw new ArgumentNullException(nameof(providerRow));
            }

            DataColumn assemblyQualifiedNameColumn = providerRow.Table.Columns[AssemblyQualifiedNameColumnName];

            if (assemblyQualifiedNameColumn is null)
            {
                throw new ArgumentException(SubSonicErrorMessages.DbProviderFactoryNoAssemblyQualifiedName);
            }

            string assemblyQualifiedName = providerRow[assemblyQualifiedNameColumn] as string;

            if (assemblyQualifiedName.IsNotNullOrEmpty())
            {
                return GetFactoryInstance(Type.GetType(assemblyQualifiedName));
            }

            return null;
        }

        public static DbProviderFactory GetFactory(DbConnection connection)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            Type dbConnectionClass = connection.GetType();

            PropertyInfo provider = dbConnectionClass.GetProperty(nameof(DbProviderFactory), BindingFlags.NonPublic);

            return (DbProviderFactory)provider.GetValue(connection);
        }

        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            return registeredFactories[providerInvariantName];
        }

        public static DataTable GetFactoryClasses()
        {
            DataColumn
                nameColumn = new DataColumn(NameColumnName, typeof(string)) { ReadOnly = true },
                descriptionColumn = new DataColumn(DescriptionColumnName, typeof(string)) { ReadOnly = true },
                invariantNameColumn = new DataColumn(InvariantNameColumnName, typeof(string)) { ReadOnly = true },
                assemblyQualifiedNameColumn = new DataColumn(AssemblyQualifiedNameColumnName, typeof(string)) { ReadOnly = true };

            DataTable result = new DataTable(ProviderGroupColumnName) { Locale = CultureInfo.InvariantCulture };
            result.Columns.AddRange(new[] { nameColumn, descriptionColumn, invariantNameColumn, assemblyQualifiedNameColumn });
            result.PrimaryKey = new[] { invariantNameColumn };

            

            foreach(var kvp in registeredFactories)
            {
                Type providerFactoryClass = kvp.Value.GetType();

                DataRow @new = result.NewRow();

                @new[InvariantNameColumnName] = kvp.Key;
                @new[AssemblyQualifiedNameColumnName] = providerFactoryClass.AssemblyQualifiedName;
                @new[NameColumnName] = providerFactoryClass.Name;
                @new[DescriptionColumnName] = providerFactoryClass.GetCustomAttribute<DescriptionAttribute>().IsNotNull(x => x.Description, string.Empty);

                result.Rows.Add(@new);
            }

            return result;
        }

        public static IEnumerable<string> GetProviderInvariantNames()
        {
            return registeredFactories.Keys.ToList();
        }

        public static void RegisterFactory(string providerInvariantName, DbProviderFactory factory)
        {
            if (providerInvariantName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(providerInvariantName));
            }

            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            registeredFactories[providerInvariantName] = factory;
        }

        public static void RegisterFactory(string providerInvariantName, Type providerFactoryClass)
        {
            if (providerFactoryClass is null)
            {
                throw new ArgumentNullException(nameof(providerFactoryClass));
            }

            RegisterFactory(providerInvariantName, GetFactoryInstance(providerFactoryClass));
        }

        public static void RegisterFactory(string providerInvariantName, string factoryTypeAssemblyQualifiedName)
        {
            RegisterFactory(providerInvariantName, Type.GetType(factoryTypeAssemblyQualifiedName));
        }

        public static bool TryGetFactory(string providerInvariantName, out DbProviderFactory factory)
        {
            if (providerInvariantName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(providerInvariantName));
            }

            factory = GetFactory(providerInvariantName);

            return !(factory is null);
        }

        public static bool UnregisterFactory(string providerInvariantName)
        {
            return providerInvariantName.IsNotNullOrEmpty() && registeredFactories.TryRemove(providerInvariantName, out _);
        }

        private static DbProviderFactory GetFactoryInstance(Type providerFactoryClass)
        {
            if (providerFactoryClass is null)
            {
                throw new ArgumentNullException(nameof(providerFactoryClass));
            }

            if (!providerFactoryClass.IsSubclassOf(typeof(DbProviderFactory)))
            {
                throw new InvalidOperationException(SubSonicErrorMessages.DbProviderFactorySubClassError.Format(providerFactoryClass.Name, nameof(DbProviderFactory)));
            }

            FieldInfo instance = providerFactoryClass.GetField(InstanceFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);

            if (instance is null)
            {
                throw new InvalidOperationException(SubSonicErrorMessages.DbProviderFactoryNoInstance.Format(providerFactoryClass.Name));
            }

            DbProviderFactory factory = (DbProviderFactory)instance.GetValue(null);

            if(factory is null)
            {
                throw new NullReferenceException();
            }

            return factory;
        }
    }
}

#endif
