using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace SubSonic.Data.DynamicProxies
{
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;

    public delegate void Proxy();

    internal class DynamicProxyBuilder<TEntity>
    {
        private readonly TypeBuilder typeBuilder;
        private readonly Type baseType;
        private readonly DbContext dbContext;

        private FieldBuilder fieldDbContextAccessor;
        private FieldBuilder fieldIsDirty;
        private FieldBuilder fieldIsDeleted;
        private FieldBuilder fieldIsNew;

        public static class ProxyStub
        {
            public static Func<TEntity, DbContextAccessor, object[]> KeyData { get; } =
                (entity, context) =>
                {
                    string[] keys = context.Model
                        .GetEntityModel<TEntity>()
                        .GetPrimaryKey()
                        .ToArray();

                    return context.GetKeyData(entity, keys);
                };
            public static Func<TEntity, TEntity> Data { get; } = (entity) => entity;
            public static Func<TEntity, Type> ModelType { get; } = 
                (entity) =>
                {
                    return entity.GetType().BaseType;
                };
            public static Action<TEntity, IEnumerable<object>> SetKeyData { get; } =
                (entity, keyData) =>
                {
                    string[] keys = DbContext.DbModel
                        .GetEntityModel<TEntity>()
                        .GetPrimaryKey()
                        .ToArray();

                    for (int i = 0, n = keys.Length; i < n; i++)
                    {
                        typeof(TEntity).GetProperty(keys[i]).SetValue(entity, keyData.ElementAt(i));
                    }
                };
            public static Action<IEntityProxy<TEntity>, IEntityProxy<TEntity>> SetDbComputedProperties { get; } =
                (entity, fromDb) =>
                {
                    IDbEntityModel model = DbContext.DbModel.GetEntityModel<TEntity>();

                    //foreach (IDbEntityProperty property in model.Properties)
                    Parallel.ForEach(model.Properties, property =>
                    {
                        if (!property.IsComputed)
                        {
                            //continue;
                            return;
                        }

                        PropertyInfo info = typeof(TEntity).GetProperty(property.PropertyName);

                        info.SetValue(entity.Data, info.GetValue(fromDb.Data));
                    }
                    );
                };

            public static Action<IEntityProxy<TEntity>> EnsureForeignKeys { get; } =
                (entity) =>
                {
                    IDbEntityModel model = DbContext.DbModel.GetEntityModel<TEntity>();

                    //foreach (IDbEntityProperty property in model.Properties)
                    Parallel.ForEach(model.Properties, property =>
                    {
                        if (property.EntityPropertyType != DbEntityPropertyType.Navigation)
                        {
                            //continue;
                            return;
                        }

                        PropertyInfo info = typeof(TEntity).GetProperty(property.PropertyName);

                        info.SetValue(entity.Data, info.GetValue(entity.Data));
                    }
                    );
                };
        }

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

            #region implement the IEntityProxy interface
            BuildKeyDataProperty();
            BuildIsDirtyProperty();
            BuildIsNewProperty();
            BuildIsDeletedProperty();
            BuildModelTypeProperty();

            BuildSetKeyDataMethod();
            #endregion

            #region implement Data per the IEntityProxy<TEntity> interface
            BuildDataProperty();
            BuildSetDbComputedPropertiesMethod();
            BuildEnsureForeignKeysMethod();
            #endregion

            foreach (IDbEntityProperty property in model.Properties)
            {
                if (property.EntityPropertyType.In(DbEntityPropertyType.Unknown, DbEntityPropertyType.DAL))
                {
                    continue;
                }

                PropertyInfo info = baseType.GetProperty(property.PropertyName);

                if(!info.GetMethod.IsVirtual)
                {   // we cannot override this property
                    continue;
                }

                if (property.EntityPropertyType == DbEntityPropertyType.Navigation || property.EntityPropertyType == DbEntityPropertyType.Collection)
                {
                    BuildOverriddenProperty(property.PropertyName, property.PropertyType, property.EntityPropertyType == DbEntityPropertyType.Collection);
                }
                else if (property.EntityPropertyType == DbEntityPropertyType.Value)
                {
                    BuildValueOverriddenProperty(property.PropertyName, property.PropertyType);
                }
            }
#if NETSTANDARD2_1
            return typeBuilder.CreateType();
#elif NETSTANDARD2_0
            return typeBuilder.CreateTypeInfo().AsType();
#endif
        }

        private void BuildDataProperty()
        {
            PropertyInfo property = BuildProperty(entity => entity.Data, getter: () => ProxyStub.Data);
        }

        private void BuildKeyDataProperty()
        {
            BuildProperty((Proxy) => Proxy.KeyData, getter: () => ProxyStub.KeyData);
        }

        private void BuildIsDirtyProperty()
        {
            BuildProperty((Proxy) => Proxy.IsDirty, fieldIsDirty);
        }

        private void BuildIsNewProperty()
        {
            PropertyInfo property = BuildProperty((Proxy) => Proxy.IsNew, fieldIsNew);
        }

        private void BuildIsDeletedProperty()
        {
            BuildProperty((Proxy) => Proxy.IsDeleted, fieldIsDeleted);
        }

        private void BuildModelTypeProperty()
        {
            BuildProperty((Proxy) => Proxy.ModelType, getter: () => ProxyStub.ModelType);
        }

        private void BuildSetDbComputedPropertiesMethod()
        {
            MethodInfo info = BuildMethod<Action<IEntityProxy<TEntity>>>((proxy) => proxy.SetDbComputedProperties, () => ProxyStub.SetDbComputedProperties);

            typeBuilder.DefineMethodOverride(info, typeof(IEntityProxy<TEntity>).GetMethod(info.Name));
        }

        private void BuildEnsureForeignKeysMethod()
        {
            MethodInfo info = BuildMethod<Action>((proxy) => proxy.EnsureForeignKeys, () => ProxyStub.EnsureForeignKeys);

            typeBuilder.DefineMethodOverride(info, typeof(IEntityProxy<TEntity>).GetMethod(info.Name));
        }

        private void BuildSetKeyDataMethod()
        {
            MethodInfo info = BuildMethod<Action<IEnumerable<object>>>((proxy) => proxy.SetKeyData, () => ProxyStub.SetKeyData);

            typeBuilder.DefineMethodOverride(info, typeof(IEntityProxy).GetMethod(info.Name));
        }

        private void BuildProxyConstructor()
        {
            fieldDbContextAccessor = typeBuilder.DefineField($"_dbContextAccessor", typeof(DbContextAccessor), FieldAttributes.Private);
            fieldIsDeleted = typeBuilder.DefineField($"_isDeleted", typeof(bool), FieldAttributes.Private);
            fieldIsDirty = typeBuilder.DefineField($"_isDirty", typeof(bool), FieldAttributes.Private);
            fieldIsNew = typeBuilder.DefineField($"_isNew", typeof(bool), FieldAttributes.Private);
            
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

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldc_I4, 1);
            iLGenerator.Emit(OpCodes.Stfld, fieldIsNew);

            iLGenerator.Emit(OpCodes.Ret);
        }

        private void BuildValueOverriddenProperty(string propertyName, Type propertyType)
        {
            _ = typeBuilder.DefineProperty(
                    propertyName,
                    PropertyAttributes.None,
                    propertyType,
                    Type.EmptyTypes);

            MethodAttributes methodAttributesForGetAndSet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

            MethodBuilder
                getMethod = typeBuilder.DefineMethod($"get_{propertyName}", methodAttributesForGetAndSet, propertyType, Type.EmptyTypes),
                setMethod = typeBuilder.DefineMethod($"set_{propertyName}", methodAttributesForGetAndSet, null, new Type[] { propertyType });

            MethodInfo
                onChange = fieldDbContextAccessor.FieldType
                    .GetMethod("OnPropertyChanged", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { baseType }),
                getter = baseType.GetProperty(propertyName).GetGetMethod(),
                setter = baseType.GetProperty(propertyName).GetSetMethod(); 

            ILGenerator
                iLGetGenerator = getMethod.GetILGenerator(),
                iLSetGenerator = setMethod.GetILGenerator();

#region getter
            iLGetGenerator.Emit(OpCodes.Ldarg_0);                                                       // this
            iLGetGenerator.Emit(OpCodes.Call, getter);                                                  // propertyField
            iLGetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getMethod, getter);
#endregion
#region setter
            iLSetGenerator.Emit(OpCodes.Ldarg_0);
            iLSetGenerator.Emit(OpCodes.Ldarg_1);
            iLSetGenerator.Emit(OpCodes.Call, setter);

            iLSetGenerator.Emit(OpCodes.Ldarg_0);                           // this
            iLSetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);     // field variable _dbContextAccessor
            iLSetGenerator.Emit(OpCodes.Ldarg_0);                           // this ptr as the first parameter
            iLSetGenerator.EmitCall(OpCodes.Call, onChange, null);          // call the OnPropertyChanged on the DBContextAccessor object

            iLSetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(setMethod, setter);
#endregion
        }

        private void BuildOverriddenProperty(string propertyName, Type propertyType, bool isCollection)
        {
            _ = typeBuilder.DefineProperty(
                    propertyName,
                    PropertyAttributes.None,
                    propertyType,
                    Type.EmptyTypes);

            MethodAttributes methodAttributesForGetAndSet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

            MethodBuilder
                getMethod = typeBuilder.DefineMethod($"get_{propertyName}", methodAttributesForGetAndSet, propertyType, Type.EmptyTypes),
                setMethod = typeBuilder.DefineMethod($"set_{propertyName}", methodAttributesForGetAndSet, null, new Type[] { propertyType });

            ILGenerator
                iLGetGenerator = getMethod.GetILGenerator(),
                iLSetGenerator = setMethod.GetILGenerator();

            MethodInfo
                getter = baseType.GetProperty(propertyName).GetGetMethod(),
                setter = baseType.GetProperty(propertyName).GetSetMethod();

#region getter
            GenerateILForGetter(iLGetGenerator, getter, setter, propertyType, propertyName, isCollection);

            typeBuilder.DefineMethodOverride(getMethod, getter);
#endregion
#region setter
            GenerateILForSetter(iLSetGenerator, setter, propertyType, propertyName, isCollection);

            typeBuilder.DefineMethodOverride(setMethod, setter);
#endregion
        }

        private void GenerateILForGetter(ILGenerator generator, MethodInfo getter, MethodInfo setter, Type propertyType, string propertyName, bool isCollection)
        {
            Type internalExt = typeof(InternalExtensions);

            MethodInfo
                load = fieldDbContextAccessor.FieldType
                    .GetMethod(!isCollection ? "LoadProperty" : "LoadCollection", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { baseType }.Union(propertyType.BuildGenericArgumentTypes()).ToArray()),
                isForeignKeyDefaultValue = fieldDbContextAccessor.FieldType
                    .GetMethod("IsForeignKeyPropertySetToDefaultValue", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(baseType),
                isDefaultValue = internalExt.GetMethods()
                    .Where(info =>
                        info.Name.Equals("IsDefaultValue", StringComparison.OrdinalIgnoreCase) && info.IsGenericMethod)
                    .Single(),
                isNull = internalExt.GetMethod("IsNull", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);

            LocalBuilder
                propertyInfo = generator.DeclareLocal(typeof(PropertyInfo)),
                count = null;

            Label
                fieldIsNotNullOrForeignKeyIsDefault = generator.DefineLabel(),
                fieldIsNull = generator.DefineLabel(),
                fieldCountIsNotZero = generator.DefineLabel();

            if (isCollection)
            {
                count = generator.DeclareLocal(typeof(int));
            }

            generator.Emit(OpCodes.Ldarg_0);                                                                               // this
            generator.EmitCall(OpCodes.Callvirt, typeof(object).GetMethod("GetType"), null);                                   // call GetType method
            generator.Emit(OpCodes.Ldstr, propertyName);                                                                   // push new string of propertyName
            generator.EmitCall(OpCodes.Callvirt, typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }), null);       // call GetProperty with the propertyName as the parameter
            generator.Emit(OpCodes.Stloc, propertyInfo);                                                                   // store PropertyInfo object in the local variable propertyInfo

            generator.Emit(OpCodes.Ldarg_0);                                                       // this
            generator.Emit(OpCodes.Call, getter);                                                  // propertyField
            generator.EmitCall(OpCodes.Call, isNull, null);                                        // use the static extension method IsNull

            if (!isCollection)
            {
                generator.Emit(OpCodes.Brfalse_S, fieldIsNotNullOrForeignKeyIsDefault);                 // value is not null

                generator.Emit(OpCodes.Ldarg_0);                                   // this
                generator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);             // field variable _dbContextAccessor
                generator.Emit(OpCodes.Ldarg_0);                                   // this ptr as the first parameter
                generator.Emit(OpCodes.Ldloc, propertyInfo);                       // local variable propertyInfo as the second parameter
                generator.EmitCall(OpCodes.Call, isForeignKeyDefaultValue, null);  // call the IsForeignKeyPropertySetToDefaultValue on the DBContextAccessor object
                generator.Emit(OpCodes.Brtrue_S, fieldIsNotNullOrForeignKeyIsDefault);
            }
            else
            {
                generator.Emit(OpCodes.Brtrue_S, fieldIsNull); // value is null
            }

            if (isCollection)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, getter);
                generator.EmitCall(OpCodes.Callvirt, propertyType.GetProperty("Count").GetMethod, null);
                generator.Emit(OpCodes.Stloc, count);

                generator.Emit(OpCodes.Ldloc, count);
                generator.EmitCall(OpCodes.Call, isDefaultValue.MakeGenericMethod(typeof(int)), null);
                generator.Emit(OpCodes.Brfalse_S, fieldCountIsNotZero);
                generator.MarkLabel(fieldIsNull);
            }

            generator.Emit(OpCodes.Ldarg_0);                        // this
            generator.Emit(OpCodes.Dup);                            // Duplicate the top of the stack -> this
            generator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);  // field variable _dbContextAccessor
            generator.Emit(OpCodes.Ldarg_0);                        // this ptr as the first parameter
            generator.Emit(OpCodes.Ldloc, propertyInfo);            // local variable propertyInfo as the second parameter
            generator.EmitCall(OpCodes.Call, load, null);           // call the LoadProperty or LoadCollection on the DBContextAccessor object
            generator.Emit(OpCodes.Call, setter);                   // store the return in the propertyField

            if (isCollection)
            {
                generator.MarkLabel(fieldCountIsNotZero);
            }
            else
            {
                generator.MarkLabel(fieldIsNotNullOrForeignKeyIsDefault);           // jump here when propertyField is not null
            }

            generator.Emit(OpCodes.Ldarg_0);                                        // this
            generator.Emit(OpCodes.Call, getter);                                   // propertyField
            generator.Emit(OpCodes.Ret);
        }

        private void GenerateILForSetter(ILGenerator generator, MethodInfo method, Type propertyType, string propertyName, bool isCollection)
        {
            MethodInfo
                set = fieldDbContextAccessor.FieldType
                    .GetMethod("SetForeignKeyProperty", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { baseType }.Union(propertyType.BuildGenericArgumentTypes()).ToArray());

            LocalBuilder
                propertyInfo = generator.DeclareLocal(typeof(PropertyInfo));

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, method);

            if (!isCollection)
            {
                generator.Emit(OpCodes.Ldarg_0);                                                                               // this
                generator.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);                                   // call GetType method
                generator.Emit(OpCodes.Ldstr, propertyName);                                                                   // push new string of propertyName
                generator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }), null);       // call GetProperty with the propertyName as the parameter
                generator.Emit(OpCodes.Stloc, propertyInfo);                                                                   // store PropertyInfo object in the local variable propertyInfo

                generator.Emit(OpCodes.Ldarg_0);                           // this
                generator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);     // field variable _dbContextAccessor
                generator.Emit(OpCodes.Ldarg_0);                           // this ptr as the first parameter
                generator.Emit(OpCodes.Ldloc, propertyInfo);               // local variable propertyInfo as the second parameter
                generator.EmitCall(OpCodes.Call, set, null);               // call the SetForeignKeyProperty on the DBContextAccessor object
            }

            generator.Emit(OpCodes.Ret);
        }

        public PropertyInfo BuildProperty<TProperty>(Expression<Func<IEntityProxy<TEntity>, TProperty>> selector, FieldBuilder field = null, Expression<Func<object>> getter = null, Expression<Func<object>> setter = null)
        {
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            MemberExpression member = selector.Body as MemberExpression;

            if (member.IsNotNull())
            {
                MethodAttributes
                    methodAttributesForGet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    methodAttributesForSet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                PropertyInfo property = member.Member as PropertyInfo;

                FieldBuilder _field = null;
                if (getter.IsNull() || setter.IsNotNull())
                {
                    _field = field ?? typeBuilder.DefineField($"_{property.Name}", property.PropertyType, FieldAttributes.Private);
                }

                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                        property.Name,
                        PropertyAttributes.None,
                        property.PropertyType,
                        Type.EmptyTypes);

                bool
                    isInterface = true;

                string
                    getterName = property.GetMethod.Name,
                    setterName = property.CanWrite ? property.SetMethod.Name : $"set_{property.Name}";

                MemberExpression
                    _getter = getter?.Body as MemberExpression,
                    _setter = setter?.Body as MemberExpression;

                MethodInfo getInvoke = null;

                Type
                    getInvokeType = null,
                    setInvokeType = null;

                if (_getter?.Member is PropertyInfo)
                {
                    getInvokeType = ((PropertyInfo)_getter.Member).PropertyType;
                    getInvoke = getInvokeType.GetMethod("Invoke");
                }
                else if (_getter?.Member is FieldInfo)
                {
                    getInvokeType = ((FieldInfo)_getter.Member).FieldType;
                    getInvoke = getInvokeType.GetMethod("Invoke");
                }
                else if(_getter.IsNotNull())
                {
                    throw new ArgumentException("", nameof(getter));
                }

                MethodInfo setInvoke = null;

                if (_setter?.Member is PropertyInfo)
                {
                    setInvokeType = ((PropertyInfo)_setter.Member).PropertyType;
                    setInvoke = setInvokeType.GetMethod("Invoke");
                }
                else if (_setter?.Member is FieldInfo)
                {
                    setInvokeType = ((FieldInfo)_setter.Member).FieldType;
                    setInvoke = setInvokeType.GetMethod("Invoke");
                }
                else if (_setter.IsNotNull())
                {
                    throw new ArgumentException("", nameof(setter));
                }

                if (isInterface)
                {
                    if (property.CanRead)
                    {
                        methodAttributesForGet |= MethodAttributes.Virtual;
                    }
                    if (property.CanWrite)
                    {
                        methodAttributesForSet |= MethodAttributes.Virtual;
                    }
                }

                MethodBuilder
                    getMethod = property.CanRead ? typeBuilder.DefineMethod(getterName, methodAttributesForGet, property.PropertyType, Type.EmptyTypes) : null,
                    setMethod = property.CanWrite ? typeBuilder.DefineMethod(setterName, methodAttributesForSet, null, new Type[] { property.PropertyType }) : null;

                ILGenerator
                    iLGetGenerator = getMethod?.GetILGenerator(),
                    iLSetGenerator = setMethod?.GetILGenerator();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                Type[] arguments = Array.Empty<Type>();
#pragma warning restore IDE0059 // Unnecessary assignment of a value

                if (property.CanRead)
                {
#region getter
                    if (getter.IsNull())
                    {
                        iLGetGenerator.Emit(OpCodes.Ldarg_0);
                        iLGetGenerator.Emit(OpCodes.Ldfld, _field);
                    }
                    else
                    {
                        arguments = getInvokeType.GetGenericArguments();

                        if (_getter.Member is PropertyInfo propertyInfo)
                        {
                            iLGetGenerator.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                        }
                        else if (_getter.Member is FieldInfo fieldInfo)
                        {
                            iLGetGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
                        }

                        for(int i = 0; i < arguments.Length - 1; i++)
                        {
                            if (arguments[i] == typeof(TEntity) || arguments[i].IsSubclassOf(typeof(TEntity)))
                            {
                                iLGetGenerator.Emit(OpCodes.Ldarg_0);
                            }
                            else if (arguments[i] == typeof(DbContextAccessor))
                            {
                                iLGetGenerator.Emit(OpCodes.Ldarg_0);
                                iLGetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);
                            }
                        }
                        iLGetGenerator.Emit(OpCodes.Callvirt, getInvoke);
                    }
                    iLGetGenerator.Emit(OpCodes.Ret);
#endregion

                    if (isInterface)
                    {
                        typeBuilder.DefineMethodOverride(getMethod, InternalExtensions.GetProperty(typeof(IEntityProxy<TEntity>), propertyBuilder.Name).GetMethod);
                    }
                }

                if (property.CanWrite)
                {
#region setter
                    iLSetGenerator.Emit(OpCodes.Ldarg_0);
                    iLSetGenerator.Emit(OpCodes.Ldarg_1);
                    iLSetGenerator.Emit(OpCodes.Stfld, _field);

                    if(setter.IsNotNull())
                    {
                        arguments = setInvokeType.GetGenericArguments();

                        if (_setter.Member is PropertyInfo propertyInfo)
                        {
                            iLSetGenerator.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                        }
                        else if (_setter.Member is FieldInfo fieldInfo)
                        {
                            iLSetGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
                        }

                        for (int i = 0; i < arguments.Length; i++)
                        {
                            if (arguments[i] == typeof(TEntity) || arguments[i].IsSubclassOf(typeof(TEntity)))
                            {
                                iLSetGenerator.Emit(OpCodes.Ldarg_0);
                            }
                            else if (arguments[i] == typeof(DbContextAccessor))
                            {
                                iLSetGenerator.Emit(OpCodes.Ldarg_0);
                                iLSetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);
                            }
                        }
                        iLSetGenerator.Emit(OpCodes.Callvirt, setInvoke);
                    }

                    iLSetGenerator.Emit(OpCodes.Ret);
#endregion

                    if (isInterface)
                    {
                        typeBuilder.DefineMethodOverride(setMethod, InternalExtensions.GetProperty(typeof(IEntityProxy<TEntity>), propertyBuilder.Name).SetMethod);
                    }
                }

                return propertyBuilder;
            }

            return null;
        }

        public MethodBuilder BuildMethod<TMethodInfo>(
            Expression<Func<IEntityProxy<TEntity>, TMethodInfo>> method,
            Expression<Func<object>> @delegate)
        {
            MemberExpression
                body = @delegate.Body as MemberExpression;

            Type type;

            if (body.Member is PropertyInfo pi)
            {
                type = pi.PropertyType;
            }
            else if (body.Member is FieldInfo fi)
            {
                type = fi.FieldType;
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (method.Body is UnaryExpression unary)
            {
                if (unary.Operand is MethodCallExpression call)
                {
                    if (call.Object is ConstantExpression constant)
                    {
                        MethodInfo info = constant.Value as MethodInfo;

                        MethodBuilder builder = typeBuilder.DefineMethod(
                            info.Name,
                            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                            info.ReturnType,
                            info.GetParameters().Select(x => x.ParameterType).ToArray());

                        ILGenerator iL = builder.GetILGenerator();

                        MethodInfo invoke = type.GetMethod("Invoke");

                        if (body.Member is PropertyInfo propertyInfo)
                        {
                            iL.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
                        }
                        else if (body.Member is FieldInfo fieldInfo)
                        {
                            iL.Emit(OpCodes.Ldsfld, fieldInfo);
                        }

                        iL.Emit(OpCodes.Ldarg_0);
                        for (int i = 0, n = info.GetParameters().Length; i < n; i++)
                        {
                            iL.Emit((OpCode)typeof(OpCodes).GetField($"Ldarg_{(i + 1)}").GetValue(null));
                        }
                        iL.Emit(OpCodes.Callvirt, invoke);
                        iL.Emit(OpCodes.Ret);

                        return builder;
                    }
                }
            }
            throw new NotSupportedException();
        }
    }
}
