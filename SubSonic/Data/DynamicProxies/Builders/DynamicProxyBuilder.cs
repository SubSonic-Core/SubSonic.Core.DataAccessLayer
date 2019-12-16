using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SubSonic.Data.DynamicProxies
{
    internal class DynamicProxyBuilder
    {
        private readonly TypeBuilder typeBuilder;
        private readonly Type baseType;
        private readonly DbContext dbContext;

        private FieldBuilder fieldDbContextAccessor;
        private FieldBuilder fieldIsDirty;

        public DynamicProxyBuilder(TypeBuilder typeBuilder, Type baseType, DbContext dbContext)
        {
            this.typeBuilder = typeBuilder ?? throw new ArgumentNullException(nameof(typeBuilder));
            this.baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Type CreateType()
        {
            BuildProxyConstructor();

            IDbEntityModel model = dbContext.Model.GetEntityModel(baseType);

            foreach (IDbEntityProperty property in model.Properties)
            {
                PropertyInfo info = baseType.GetProperty(property.PropertyName);

                if(!info.GetMethod.IsVirtual)
                {   // we cannot override this property
                    continue;
                }

                if (property.EntityPropertyType == DbEntityPropertyType.Navigation || property.EntityPropertyType == DbEntityPropertyType.Collection)
                {
                    BuildOverriddenProperty(property.PropertyName, property.PropertyType, property.EntityPropertyType == DbEntityPropertyType.Collection);
                }
            }

            #region implement IsDirty per the IEntityProxy interface
            BuildIsDirtyProperty();
            #endregion

            return typeBuilder.CreateType();
        }

        private void BuildIsDirtyProperty()
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                    "IsDirty",
                    PropertyAttributes.None,
                    typeof(bool),
                    Type.EmptyTypes);

            MethodAttributes methodAttributesForGetAndSet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            MethodBuilder
                getMethod = typeBuilder.DefineMethod($"get_IsDirty", methodAttributesForGetAndSet | MethodAttributes.Virtual, typeof(bool), Type.EmptyTypes),
                setMethod = typeBuilder.DefineMethod($"set_IsDirty", methodAttributesForGetAndSet, null, new Type[] { typeof(bool) });

            ILGenerator
                iLGetGenerator = getMethod.GetILGenerator(),
                iLSetGenerator = setMethod.GetILGenerator();

            #region getter
            iLGetGenerator.Emit(OpCodes.Ldarg_0);
            iLGetGenerator.Emit(OpCodes.Ldfld, fieldIsDirty);
            iLGetGenerator.Emit(OpCodes.Ret);
            #endregion
            #region setter
            iLSetGenerator.Emit(OpCodes.Ldarg_0);
            iLSetGenerator.Emit(OpCodes.Ldarg_1);
            iLSetGenerator.Emit(OpCodes.Stfld, fieldIsDirty);
            iLGetGenerator.Emit(OpCodes.Ret);
            #endregion
        }

        private void BuildProxyConstructor()
        {
            fieldDbContextAccessor = typeBuilder.DefineField($"_dbContextAccessor", typeof(DbContextAccessor), FieldAttributes.Private);
            fieldIsDirty = typeBuilder.DefineField($"_isDirty", typeof(bool), FieldAttributes.Private);

            ConstructorInfo baseCtor = baseType.GetConstructor(Type.EmptyTypes);

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new Type[] { typeof(DbContextAccessor) });

            ILGenerator iLGenerator = constructorBuilder.GetILGenerator();

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, baseCtor);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Stfld, fieldDbContextAccessor);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void BuildOverriddenProperty(string propertyName, Type propertyType, bool isCollection)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                    propertyName,
                    PropertyAttributes.None,
                    propertyType,
                    Type.EmptyTypes);

            MethodAttributes methodAttributesForGetAndSet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual ;

            MethodBuilder
                getMethod = typeBuilder.DefineMethod($"get_{propertyName}", methodAttributesForGetAndSet, propertyType, Type.EmptyTypes),
                setMethod = typeBuilder.DefineMethod($"set_{propertyName}", methodAttributesForGetAndSet, null, new Type[] { propertyType });

            ILGenerator
                iLGetGenerator = getMethod.GetILGenerator(),
                iLSetGenerator = setMethod.GetILGenerator();

            Type internalExt = typeof(InternalExtensions);

            MethodInfo
                load = fieldDbContextAccessor.FieldType
                    .GetMethod(!isCollection ? "LoadProperty" : "LoadCollection", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { baseType }.Union(propertyType.BuildGenericArgumentTypes()).ToArray()),
                set = fieldDbContextAccessor.FieldType
                    .GetMethod("SetForeignKeyProperty", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { baseType }.Union(propertyType.BuildGenericArgumentTypes()).ToArray()),
                isForeignKeyDefaultValue = fieldDbContextAccessor.FieldType
                    .GetMethod("IsForeignKeyPropertySetToDefaultValue", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(baseType),
                isNull = internalExt.GetMethod("IsNull", BindingFlags.Public | BindingFlags.Static , null, new[] { typeof(object) }, null),
                isDefaultValue = internalExt.GetMethods()
                    .Where(info =>
                        info.Name.Equals("IsDefaultValue", StringComparison.OrdinalIgnoreCase) && info.IsGenericMethod)
                    .Single(),
                getter = baseType.GetProperty(propertyName).GetGetMethod(),
                setter = baseType.GetProperty(propertyName).GetSetMethod();

            Label 
                fieldIsNotNullOrForeignKeyIsDefault = iLGetGenerator.DefineLabel(),
                fieldIsNull = iLGetGenerator.DefineLabel(),
                fieldCountIsNotZero = iLGetGenerator.DefineLabel();

            LocalBuilder
                propertyInfo = iLGetGenerator.DeclareLocal(typeof(PropertyInfo)),
                count = null;
            #region getter
            if (isCollection)
            {
                count = iLGetGenerator.DeclareLocal(typeof(int));
            }

            iLGetGenerator.Emit(OpCodes.Ldarg_0);                                                                               // this
            iLGetGenerator.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);                                   // call GetType method
            iLGetGenerator.Emit(OpCodes.Ldstr, propertyName);                                                                   // push new string of propertyName
            iLGetGenerator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }), null);       // call GetProperty with the propertyName as the parameter
            iLGetGenerator.Emit(OpCodes.Stloc, propertyInfo);                                                                   // store PropertyInfo object in the local variable propertyInfo

            iLGetGenerator.Emit(OpCodes.Ldarg_0);                                                       // this
            iLGetGenerator.Emit(OpCodes.Call, getter);       // propertyField
            iLGetGenerator.EmitCall(OpCodes.Call, isNull, null);                                        // use the static extension method IsNull
            if (!isCollection)
            {
                iLGetGenerator.Emit(OpCodes.Brfalse_S, fieldIsNotNullOrForeignKeyIsDefault);                 // value is not null

                iLGetGenerator.Emit(OpCodes.Ldarg_0);                                   // this
                iLGetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);             // field variable _dbContextAccessor
                iLGetGenerator.Emit(OpCodes.Ldarg_0);                                   // this ptr as the first parameter
                iLGetGenerator.Emit(OpCodes.Ldloc, propertyInfo);                       // local variable propertyInfo as the second parameter
                iLGetGenerator.EmitCall(OpCodes.Call, isForeignKeyDefaultValue, null);  // call the IsForeignKeyPropertySetToDefaultValue on the DBContextAccessor object
                iLGetGenerator.Emit(OpCodes.Brtrue_S, fieldIsNotNullOrForeignKeyIsDefault);
            }
            else
            {
                iLGetGenerator.Emit(OpCodes.Brtrue_S, fieldIsNull); // value is null
            }

            if (isCollection)
            {
                iLGetGenerator.Emit(OpCodes.Ldarg_0);
                iLGetGenerator.Emit(OpCodes.Call, getter);
                iLGetGenerator.EmitCall(OpCodes.Call, propertyType.GetProperty("Count").GetMethod, null);
                iLGetGenerator.Emit(OpCodes.Stloc, count);

                iLGetGenerator.Emit(OpCodes.Ldloc, count);
                iLGetGenerator.EmitCall(OpCodes.Call, isDefaultValue.MakeGenericMethod(typeof(int)), null);
                iLGetGenerator.Emit(OpCodes.Brfalse_S, fieldCountIsNotZero);
                iLGetGenerator.MarkLabel(fieldIsNull);
                iLGetGenerator.BeginScope();
            }
            else
            {
                iLGetGenerator.BeginScope();
            }

            {  
                iLGetGenerator.Emit(OpCodes.Ldarg_0);                           // this
                iLGetGenerator.Emit(OpCodes.Dup);                               // Duplicate the top of the stack -> this
                iLGetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);     // field variable _dbContextAccessor
                iLGetGenerator.Emit(OpCodes.Ldarg_0);                           // this ptr as the first parameter
                iLGetGenerator.Emit(OpCodes.Ldloc, propertyInfo);               // local variable propertyInfo as the second parameter
                iLGetGenerator.EmitCall(OpCodes.Call, load, null);              // call the LoadProperty or LoadCollection on the DBContextAccessor object
                iLGetGenerator.Emit(OpCodes.Call, setter);              // store the return in the propertyField
                if (isCollection)
                {
                    iLGetGenerator.MarkLabel(fieldCountIsNotZero);
                }
            }
            iLGetGenerator.EndScope();
            if (!isCollection)
            {
                iLGetGenerator.MarkLabel(fieldIsNotNullOrForeignKeyIsDefault);               // jump here when propertyField is not null
            }
            iLGetGenerator.Emit(OpCodes.Ldarg_0);   // this
            iLGetGenerator.Emit(OpCodes.Call, getter); // propertyField
            iLGetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getMethod, baseType.GetProperty(propertyName).GetMethod);
            #endregion
            #region setter
            propertyInfo = iLSetGenerator.DeclareLocal(typeof(PropertyInfo));

            iLSetGenerator.Emit(OpCodes.Ldarg_0);
            iLSetGenerator.Emit(OpCodes.Ldarg_1);
            iLSetGenerator.Emit(OpCodes.Call, baseType.GetProperty(propertyName).GetSetMethod());

            if (!isCollection)
            {
                iLSetGenerator.Emit(OpCodes.Ldarg_0);                                                                               // this
                iLSetGenerator.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);                                   // call GetType method
                iLSetGenerator.Emit(OpCodes.Ldstr, propertyName);                                                                   // push new string of propertyName
                iLSetGenerator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }), null);       // call GetProperty with the propertyName as the parameter
                iLSetGenerator.Emit(OpCodes.Stloc, propertyInfo);                                                                   // store PropertyInfo object in the local variable propertyInfo

                iLSetGenerator.Emit(OpCodes.Ldarg_0);                           // this
                iLSetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);     // field variable _dbContextAccessor
                iLSetGenerator.Emit(OpCodes.Ldarg_0);                           // this ptr as the first parameter
                iLSetGenerator.Emit(OpCodes.Ldloc, propertyInfo);               // local variable propertyInfo as the second parameter
                iLSetGenerator.EmitCall(OpCodes.Call, set, null);               // call the SetForeignKeyProperty on the DBContextAccessor object
            }

            iLSetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(setMethod, baseType.GetProperty(propertyName).SetMethod);
            #endregion
        }
    }
}
