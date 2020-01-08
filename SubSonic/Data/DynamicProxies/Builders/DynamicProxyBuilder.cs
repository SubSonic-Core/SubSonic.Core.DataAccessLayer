using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace SubSonic.Data.DynamicProxies
{
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

            return typeBuilder.CreateType();
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
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
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
            iLGetGenerator.Emit(OpCodes.Call, getter);                                                  // propertyField
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
            iLGetGenerator.Emit(OpCodes.Ldarg_0);                                       // this
            iLGetGenerator.Emit(OpCodes.Call, getter);                                  // propertyField
            iLGetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getMethod, getter);
            #endregion
            #region setter
            propertyInfo = iLSetGenerator.DeclareLocal(typeof(PropertyInfo));

            iLSetGenerator.Emit(OpCodes.Ldarg_0);
            iLSetGenerator.Emit(OpCodes.Ldarg_1);
            iLSetGenerator.Emit(OpCodes.Call, setter);

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

            typeBuilder.DefineMethodOverride(setMethod, setter);
            #endregion
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

                        if (_getter.Member is PropertyInfo)
                        {
                            iLGetGenerator.Emit(OpCodes.Call, ((PropertyInfo)_getter.Member).GetGetMethod());
                        }
                        else if (_getter.Member is FieldInfo)
                        {
                            iLGetGenerator.Emit(OpCodes.Ldsfld, (FieldInfo)_getter.Member);
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

                        if (_setter.Member is PropertyInfo)
                        {
                            iLSetGenerator.Emit(OpCodes.Call, ((PropertyInfo)_setter.Member).GetGetMethod());
                        }
                        else if (_setter.Member is FieldInfo)
                        {
                            iLSetGenerator.Emit(OpCodes.Ldsfld, (FieldInfo)_setter.Member);
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

                        if (body.Member is PropertyInfo)
                        {
                            iL.Emit(OpCodes.Call, ((PropertyInfo)body.Member).GetGetMethod());
                        }
                        else if (body.Member is FieldInfo)
                        {
                            iL.Emit(OpCodes.Ldsfld, ((FieldInfo)body.Member));
                        }

                        iL.Emit(OpCodes.Ldarg_0);
                        for (int i = 0, n = info.GetParameters().Count(); i < n; i++)
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
