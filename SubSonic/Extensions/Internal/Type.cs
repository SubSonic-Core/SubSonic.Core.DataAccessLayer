using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
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
            if(type.IsValueType)
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

        public static string GetQualifiedTypeName(this Type type)
        {
            string result;
            if(type.IsGenericType)
            {
                result = $"{type.Name}<{string.Join(',', type.GetGenericArguments().Select(t => t.Name))}>";
            }
            else
            {
                result = type.Name;
            }
            return result;
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
    }
}
