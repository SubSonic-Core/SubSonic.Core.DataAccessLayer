using System;

namespace SubSonic.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public class ProviderInvariantNameNotRegisteredException
        : Exception
    {
        public ProviderInvariantNameNotRegisteredException(string providerInvariantName, string factoryName) 
            : base(SubSonicErrorMessages.ProviderInvariantNameNotRegisteredException.Format(providerInvariantName, factoryName))
        {
        }

        public ProviderInvariantNameNotRegisteredException(string providerInvariantName, string factoryName, Exception innerException) 
            : base(SubSonicErrorMessages.ProviderInvariantNameNotRegisteredException.Format(providerInvariantName, factoryName), innerException)
        {
        }
    }
}
