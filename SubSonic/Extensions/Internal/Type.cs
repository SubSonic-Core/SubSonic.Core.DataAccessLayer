﻿using System;
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
    }
}