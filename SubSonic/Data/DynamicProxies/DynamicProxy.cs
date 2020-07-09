using Microsoft.Win32.SafeHandles;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleTo("SubSonic.DynamicProxies, PublicKey=0024000004800000940000000602000000240000525341310004000001000100754c177654d80bd8f61f259da8b891ed72cc003e5bbe17828908490c5af8edaf9ecfb0c4564987334a7b92559823275cec4d314d3b172760f83f1b08688fd66588b6673f29f860ff367d616541e49b85e609bf0255ab722a2cb8080abaf15931d509423acea0c79b57df9772b634c5a3bdc0e299fd0a6aaa21739c1b8be49ebd")]

namespace SubSonic.Data.DynamicProxies
{
    public static class DynamicProxy
    {
        private readonly static Dictionary<string, DynamicProxyWrapper> DynamicProxyCache = new Dictionary<string, DynamicProxyWrapper>();
        private static ModuleBuilder ModuleBuilder = GetModuleBuilder();

        public static Assembly DynamicAssembly => ModuleBuilder.Assembly;

        public static TEntity CreateProxyInstanceOf<TEntity>(DbContext dbContext)
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

        public static TEntity MapInstanceOf<TEntity>(DbContext context, IEntityProxy<TEntity> instance)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            TEntity entity = CreateProxyInstanceOf<TEntity>(context);

            entity.Map(instance.Data);  

            if (entity is IEntityProxy<TEntity> proxy)
            {
                proxy.IsDirty = instance.IsDirty;
                proxy.IsNew = instance.IsNew;
            }

            return entity;
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

        internal static ModuleBuilder GetModuleBuilder()
        {
            if (ModuleBuilder is null)
            {
                AssemblyName
                    executingName = Assembly.GetExecutingAssembly().GetName(),
                    assemblyName = new AssemblyName("SubSonic.DynamicProxies");

                assemblyName.KeyPair = executingName.KeyPair;
                assemblyName.SetPublicKey(executingName.GetPublicKey());
                
                AssemblyBuilder DynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);

                ModuleBuilder = DynamicAssembly.DefineDynamicModule("DynamicProxiesTypeGenerator");
            }

            return ModuleBuilder;
        }

        internal static Type BuildDerivedTypeFrom<TEntity>(DbContext dbContext)
        {
            Type baseType = typeof(TEntity);

            DynamicProxyBuilder<TEntity> proxyBuilder = new DynamicProxyBuilder<TEntity>(ModuleBuilder.DefineType(
                $"{DynamicAssembly.FullName}.{baseType.Name}",
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType, new[] { typeof(IEntityProxy<>).MakeGenericType(baseType), typeof(IEntityProxy) }), baseType, dbContext);

            return proxyBuilder.CreateType();
        }            
    }
}

