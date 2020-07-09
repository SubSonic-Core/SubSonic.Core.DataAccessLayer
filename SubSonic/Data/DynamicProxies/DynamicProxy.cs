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

[assembly: InternalsVisibleTo("SubSonic.DynamicProxies, PublicKey=0024000004800000940000000602000000240000525341310004000001000100290cde84efb341cb2ce91d1e881e9d927bdf6f825f1165ead25ce4881956c0f3c07d6194fb35f09c9aff40946aad571dcdc19a2a040e3a59060aca1dc0a999d081577e3fb1e325115db0794a78082e098da60ab34249388e6cad907ddae1e1b40489b815e9b3cb28e10942ffa651bbf4833611fd201afe5c05c4d27c241be2a9")]

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

        private static StrongNameKeyPair GetStrongNameKeyPair()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string 
                assemblyName = assembly.GetName().Name,
                privateKeyFileForDynamicProxy = $"{assemblyName.Substring(0, assemblyName.IndexOf(".", StringComparison.Ordinal))}.DynamicProxy.pfx";

            using (var resource = assembly.GetManifestResourceStream(privateKeyFileForDynamicProxy))
            using (var reader = new BinaryReader(resource))
            {
                var data = new byte[resource.Length];

                data = reader.ReadBytes(data.Length);

                return new StrongNameKeyPair(data);
            }
        }

        private static byte[] GetPublicKey()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string
                assemblyName = assembly.GetName().Name,
                publicKeyFileForDynamicProxy = $"{assemblyName.Substring(0, assemblyName.IndexOf(".", StringComparison.Ordinal))}.DynamicProxy.PublicKey.pfx";

            using (var resource = assembly.GetManifestResourceStream(publicKeyFileForDynamicProxy))
            using (var reader = new BinaryReader(resource))
            {
                var data = new byte[resource.Length];

                data = reader.ReadBytes(data.Length);

                return data;
            }
        }

        internal static ModuleBuilder GetModuleBuilder()
        {
            if (ModuleBuilder is null)
            {
                AssemblyName assemblyName = new AssemblyName("SubSonic.DynamicProxies");

                StrongNameKeyPair kp = GetStrongNameKeyPair();

                assemblyName.KeyPair = kp;
                assemblyName.SetPublicKey(GetPublicKey());
                

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

