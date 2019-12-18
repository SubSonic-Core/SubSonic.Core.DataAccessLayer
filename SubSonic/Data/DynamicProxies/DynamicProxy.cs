using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SubSonic.DynamicProxies")]

namespace SubSonic.Data.DynamicProxies
{
    public static class DynamicProxy
    {
        private readonly static Dictionary<string, DynamicProxyWrapper> DynamicProxyCache = new Dictionary<string, DynamicProxyWrapper>();

        private readonly static AssemblyName assemblyName = new AssemblyName("SubSonic.DynamicProxies");
        private readonly static AssemblyBuilder DynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        private static ModuleBuilder ModuleBuilder = DynamicAssembly.DefineDynamicModule("DynamicProxiesTypeGenerator");

        public static TEntity CreateProxyInstanceOf<TEntity>(DbContext dbContext)
            where TEntity : class
        {
            if (dbContext is null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            DynamicProxyWrapper proxy = GetProxyWrapper<TEntity>(dbContext);

            if (dbContext.Options.EnableProxyGeneration && proxy.IsElegibleForProxy)
            {
                return (TEntity)Activator.CreateInstance(proxy.Type, new DbContextAccessor(dbContext));
            }
            else
            {
                return Activator.CreateInstance<TEntity>();
            }
        }

        public static DynamicProxyWrapper GetProxyWrapper(Type proxyType)
        {
            if (proxyType is null)
            {
                throw new ArgumentNullException(nameof(proxyType));
            }

            if (!DynamicProxyCache.ContainsKey(proxyType.FullName))
            {
                throw new InvalidOperationException();
            }

            return DynamicProxyCache[proxyType.FullName];
        }

        public static DynamicProxyWrapper GetProxyWrapper<TEntity>(DbContext dbContext)
        {
            Type baseType = typeof(TEntity);

            if (!DynamicProxyCache.ContainsKey(baseType.FullName))
            {
                DynamicProxyCache.Add(baseType.FullName, new DynamicProxyWrapper<TEntity>(dbContext));
            }

            return GetProxyWrapper(baseType);
        }

        internal static Type BuildDerivedTypeFrom<TEntity>(DbContext dbContext)
        {
            Type baseType = typeof(TEntity);

            DynamicProxyBuilder<TEntity> proxyBuilder = new DynamicProxyBuilder<TEntity>(ModuleBuilder.DefineType(
                $"{assemblyName.FullName}.{baseType.Name}",
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType, new[] { typeof(IEntityProxy<>).MakeGenericType(baseType), typeof(IEntityProxy) }), baseType, dbContext);

            return proxyBuilder.CreateType();
        }            
    }
}
