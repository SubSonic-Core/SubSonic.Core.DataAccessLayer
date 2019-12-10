#if DB_PROVIDER_NOT_DEFINED
namespace SubSonic.Infrastructure.Factories
{
    public static class DbProviderFactories
    {
        private static Dictionary<string, DbProviderFactory> ProviderFactories = new Dictionary<string, DbProviderFactory>();

        /// <summary>
        /// Returns the provider factory instance if one is registered for the given invariant name
        /// </summary>
        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            return ProviderFactories[providerInvariantName];
        }

        /// <summary>
        /// Returns the provider factory instance if one is registered for the given invariant name
        /// </summary>
        public static bool TryGetFactory(string providerInvariantName, out DbProviderFactory providerFactory)
        {
            try
            {
                providerFactory = GetFactory(providerInvariantName);
            }
            finally { }

            return providerFactory.IsNotNull();
        }

        /// <summary>
        /// Registers a provider factory using the provider factory type and an invariant name
        /// </summary>
        public static void RegisterFactory(string providerInvariantName, Type factoryType)
        {
            RegisterFactory(providerInvariantName, (DbProviderFactory)Activator.CreateInstance(factoryType));
        }

        /// <summary>
        /// Extension method to register a provider factory using the provider factory instance and 
        /// an invariant name
        /// </summary>
        public static void RegisterFactory(string providerInvariantName, DbProviderFactory factory)
        {
            if (!ProviderFactories.ContainsKey(providerInvariantName))
            {
                ProviderFactories.Add(providerInvariantName, factory);
            }
        }

        public static void RegisterFactory(string providerInvariantName, string factoryTypeAssembyQualifiedName)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (string.IsNullOrEmpty(factoryTypeAssembyQualifiedName))
            {
                throw new ArgumentException("", nameof(factoryTypeAssembyQualifiedName));
            }

            string assemblyTypeName = factoryTypeAssembyQualifiedName;
            _ = Assembly.Load(assemblyTypeName.Substring(assemblyTypeName.IndexOf(',', StringComparison.InvariantCultureIgnoreCase) + 1));

            RegisterFactory(providerInvariantName, Type.GetType(assemblyTypeName));
        }

        /// <summary>
        /// Removes the provider factory registration for the given invariant name  
        /// </summary>
        public static bool UnregisterFactory(string providerInvariantName)
        {
            if(ProviderFactories.ContainsKey(providerInvariantName))
            {
                ProviderFactories.Remove(providerInvariantName);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the invariant names for all the factories registered
        /// </summary>
        public static IEnumerable<string> GetProviderInvariantNames() => ProviderFactories.Keys;
    }
}
#endif
