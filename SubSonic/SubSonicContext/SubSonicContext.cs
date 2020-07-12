using Microsoft.Extensions.DependencyInjection;
using SubSonic.Infrastructure;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(@"SubSonic.Extensions.Test, PublicKey=
0024000004800000940000000602000000240000525341310004000001000100754c177654d80bd8f61f259da8b891ed72cc003e5bbe17828908490c5af8edaf9ecfb0c4564987334a7b92559823275cec4d314d3b172760f83f1b08688fd66588b6673f29f860ff367d616541e49b85e609bf0255ab722a2cb8080abaf15931d509423acea0c79b57df9772b634c5a3bdc0e299fd0a6aaa21739c1b8be49ebd", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(@"SubSonic.Extensions.SqlServer, PublicKey=
0024000004800000940000000602000000240000525341310004000001000100754c177654d80bd8f61f259da8b891ed72cc003e5bbe17828908490c5af8edaf9ecfb0c4564987334a7b92559823275cec4d314d3b172760f83f1b08688fd66588b6673f29f860ff367d616541e49b85e609bf0255ab722a2cb8080abaf15931d509423acea0c79b57df9772b634c5a3bdc0e299fd0a6aaa21739c1b8be49ebd", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(@"SubSonic.Tests, PublicKey=
0024000004800000940000000602000000240000525341310004000001000100754c177654d80bd8f61f259da8b891ed72cc003e5bbe17828908490c5af8edaf9ecfb0c4564987334a7b92559823275cec4d314d3b172760f83f1b08688fd66588b6673f29f860ff367d616541e49b85e609bf0255ab722a2cb8080abaf15931d509423acea0c79b57df9772b634c5a3bdc0e299fd0a6aaa21739c1b8be49ebd", AllInternalsVisible = true)]

namespace SubSonic
{
    using Collections;
    using Linq;

    public partial class SubSonicContext
        : IDisposable, IInfrastructure<IServiceProvider>
    {
        protected SubSonicContext()
        {
            Options = new SubSonicContextOptions();
            Model = new SubSonicSchemaModel(this);

            Initialize();            
        }

        private void Initialize()
        {
            ConfigureSubSonic(new DbContextOptionsBuilder(this, Options));

            RegisterDbModel(new DbModelBuilder(Model));

            SetDbSetCollections();
        }

        private void SetDbSetCollections()
        {
            foreach(PropertyInfo info in GetType().GetProperties())
            {
                if(!info.PropertyType.IsGenericType || info.PropertyType.GetGenericTypeDefinition() != typeof(ISubSonicSetCollection<>))
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

        private void RegisterDbModel(DbModelBuilder builder)
        {
            OnDbModeling(builder);

            IsDbModelReadOnly = true;

            if(Options.PreLoadHandler.IsNotNull())
            {
                Options.PreLoadHandler(Model);  
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
                    ChangeTracking.Flush();
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
