﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SubSonic.DynamicProxies")]

namespace SubSonic.Data.DynamicProxies
{
    public static class DynamicProxy
    {
        private readonly static Dictionary<string, Type> DynamicProxyCache;

        private readonly static AssemblyName assemblyName;
        private readonly static AssemblyBuilder DynamicAssembly;
        private static ModuleBuilder ModuleBuilder;

        static DynamicProxy()
        {
            assemblyName = new AssemblyName("SubSonic.DynamicProxies");

            DynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder = DynamicAssembly.DefineDynamicModule("DynamicProxiesTypeGenerator");

            DynamicProxyCache = new Dictionary<string, Type>();
        }

        public static TEntity CreateProxyInstanceOf<TEntity>(DbContext dbContext)
            where TEntity : class
        {
            Type baseType = typeof(TEntity);

            if (!DynamicProxyCache.ContainsKey(baseType.FullName))
            {
                DynamicProxyCache.Add(baseType.FullName, BuildDerivedTypeFrom(baseType));
            }

            return (TEntity)Activator.CreateInstance(DynamicProxyCache[baseType.FullName], new DbContextAccessor(dbContext));
        }

        private static Type BuildDerivedTypeFrom(Type baseType)
        {

            DynamicProxyBuilder proxyBuilder = new DynamicProxyBuilder(ModuleBuilder.DefineType(
                $"{assemblyName.FullName}.{baseType.Name}",
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType), baseType);

            return proxyBuilder.CreateType();
        }            
    }
}