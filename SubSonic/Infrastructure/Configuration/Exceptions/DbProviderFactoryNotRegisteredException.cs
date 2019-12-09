using System;

namespace SubSonic.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public class DbProviderFactoryNotRegisteredException
        : Exception
    {
        public DbProviderFactoryNotRegisteredException(string providerInvariantName) 
            : base(SubSonicErrorMessages.DbProviderFactoryNotRegisteredException.Format(providerInvariantName))
        {
        }

        public DbProviderFactoryNotRegisteredException(string providerInvariantName, Exception innerException) 
            : base(SubSonicErrorMessages.DbProviderFactoryNotRegisteredException.Format(providerInvariantName), innerException)
        {
        }
    }
}
