using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Builders;
using SubSonic.Infrastructure.Logging;
using SubSonic.Infrastructure.Providers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SubSonic.Extensions.Test", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("SubSonic.Extensions.SqlServer", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("SubSonic.Tests", AllInternalsVisible = true)]
[assembly: NeutralResourcesLanguage("en")]

namespace SubSonic
{
    public partial class DbContext
        : IDisposable, IInfrastructure<IServiceProvider>
    {
        protected DbContext()
        {
            Options = new DbContextOptions();
            Model = new DbModel(this);

            Initialize();            
        }

        private void Initialize()
        {
            ConfigureSubSonic(new DbContextOptionsBuilder(this, Options));
            
            OnDbModeling(new DbModelBuilder(Model));

            SetDbSetCollections();
        }

        private void SetDbSetCollections()
        {
            foreach(PropertyInfo info in GetType().GetProperties())
            {
                if(!info.PropertyType.IsGenericType || info.PropertyType.GetGenericTypeDefinition() != typeof(DbSetCollection<>))
                {
                    continue;
                }

                info.SetValue(this, Instance.GetService(info.PropertyType), null);
            }
        }

        private void ConfigureSubSonic(DbContextOptionsBuilder builder)
        {
            OnDbConfiguring(builder);

            IServiceCollection services = Instance.GetService<IServiceCollection>();

            if (services.IsNotNull())
            {
                this.SetupIOC(services, Options);
                

                ServiceProvider = services.BuildServiceProvider();
            }
            else
            {
                throw new MissingServiceCollectionException();
            }
        }

        protected virtual void OnDbConfiguring(DbContextOptionsBuilder builder)
        {

        }

        protected virtual void OnDbModeling(DbModelBuilder builder)
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DbContext()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
