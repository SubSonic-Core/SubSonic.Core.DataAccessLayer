using System;
using System.Globalization;

namespace SubSonic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public class ProviderInvariantNameNotRegisteredException
        : Exception
    {
        public ProviderInvariantNameNotRegisteredException(string providerInvariantName, string factoryName) 
            : base(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.ProviderInvariantNameNotRegisteredException, providerInvariantName, factoryName))
        {
        }

        public ProviderInvariantNameNotRegisteredException(string providerInvariantName, string factoryName, Exception innerException) 
            : base(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.ProviderInvariantNameNotRegisteredException, providerInvariantName, factoryName), innerException)
        {
        }
    }
}
