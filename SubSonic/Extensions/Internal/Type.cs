using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Reflection;

namespace SubSonic
{
    using Infrastructure;
    using Linq;

    internal static partial class InternalExtensions
    {
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            PropertyInfo info = null;

            if (type.IsInterface)
            {
                foreach (Type @interface in new[] { type }.Union(type.GetInterfaces()))
                {
                    if ((info = @interface.GetProperty(name)).IsNotNull())
                    {
                        return info;
                    }
                }
            }

            return info ?? type.GetProperty(name);
        }
        public static DbType GetDbType(this Type netType, bool unicode = false)
        {
            return TypeConvertor.ToDbType(netType, unicode);
        }

        public static SqlDbType GetSqlDbType(this Type netType, bool unicode = false)
        {
            return TypeConvertor.ToSqlDbType(netType, unicode);
        }
        public static bool IsBoolean(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterface(typeof(IEnumerable).FullName).IsNotNull();
        }

        public static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type GetUnderlyingType(this Type type)
        {
            return type.IsNullableType() ? Nullable.GetUnderlyingType(type) : type;
        }

        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static Type[] BuildGenericArgumentTypes(this Type type)
        {
            return type.IsGenericType ? type.GetGenericArguments() : new[] { type };
        }

        public static Type GetQualifiedType(this Type type)
        {
            return type.IsGenericType ? type.GetGenericArguments().First() : type;
        }

        public static Type FindIEnumerable(this Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                        return ienum;
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                        return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
                return FindIEnumerable(seqType.BaseType);
            return null;
        }

        public static Type GetElementType(this Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null)
                return seqType;
            return ienum.GetGenericArguments()[0];
        }

        public static bool IsScalar(this Type type)
        {
            type = type.GetUnderlyingType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Object:
                    return
                        type == typeof(DateTime) ||
                        type == typeof(DateTimeOffset) ||
                        type == typeof(decimal) ||
                        type == typeof(Guid) ||
                        type == typeof(byte[]);
                default:
                    return true;
            }
        }

        public static string GetTypeName(this Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string name = type.Name;
            name = name.Replace('+', '.');
#if NETSTANDARD2_0
            int iGeneneric = name.IndexOf('`');
#elif NETSTANDARD2_1
            int iGeneneric = name.IndexOf('`', StringComparison.CurrentCulture);
#endif

            if (iGeneneric > 0)
            {
                name = name.Substring(0, iGeneneric);
            }
            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(name);
                sb.Append("<");
                var args = type.GetGenericArguments();
                for (int i = 0, n = args.Length; i < n; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    if (type.IsGenericType)
                    {
                        sb.Append(GetTypeName(args[i]));
                    }
                }
                sb.Append(">");
                name = sb.ToString();
            }
            return name;
        }

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] types)
        {
            MethodInfo result = null;

            foreach (MethodInfo info in type
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(x =>
                            x.Name.Equals(name, StringComparison.CurrentCulture)))
            {
                if (info.IsGenericMethod)
                {
                    MethodInfo test = info.MakeGenericMethod(types[0]);

                    bool match = true;

                    ParameterInfo[] parameters = test.GetParameters();

                    for (int i = 0, cnt = types.Length; i < cnt; i++)
                    {
                        match &= parameters[i].ParameterType == types[i];
                    }

                    if (match)
                    {
                        result = test;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
