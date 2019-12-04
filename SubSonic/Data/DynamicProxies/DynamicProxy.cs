using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

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
            ModuleBuilder = DynamicAssembly.DefineDynamicModule("MainModule");

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

            return (TEntity)Activator.CreateInstance(DynamicProxyCache[baseType.FullName], dbContext);
        }

        private static Type BuildDerivedTypeFrom(Type baseType)
        {

            TypeBuilder typeBuilder = ModuleBuilder.DefineType(
                $"{assemblyName.FullName}.{baseType.Name}",
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType);

            CreateConstructor(typeBuilder, baseType);

            //foreach (PropertyInfo propertyInfo in baseType.GetProperties(BindingFlags.Public))
            //{
            //    if(!propertyInfo.GetMethod.IsVirtual)
            //    {   // do not care about what can not be overridden
            //        continue;
            //    }

            //    CreateProperty(typeBuilder, propertyInfo.Name, propertyInfo.PropertyType);
            //}

            return typeBuilder.CreateType();
        }

        private static void CreateConstructor(TypeBuilder typeBuilder, Type baseType)
        {
            FieldBuilder fieldDbContext = typeBuilder.DefineField($"_dbContext", typeof(DbContext), FieldAttributes.Private);

            ConstructorInfo baseCtor = baseType.GetConstructor(Type.EmptyTypes);

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new Type[] { typeof(DbContext) });

            ILGenerator iLGenerator = constructorBuilder.GetILGenerator();

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, baseCtor);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Stfld, fieldDbContext);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                    propertyName,
                    PropertyAttributes.None,
                    propertyType,
                    null);
        }
            
    }
}
