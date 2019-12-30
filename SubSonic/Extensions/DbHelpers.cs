﻿using SubSonic.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;

namespace SubSonic
{
    using Linq;
    public static partial class SubSonicExtensions
    {
        public static bool IsSameAs<TSource>(this IEnumerable<TSource> left, IEnumerable<TSource> right)
        {
            TSource[]
                _left = left.ToArray(),
                _right = right.ToArray();

            bool result = _left.Length == _right.Length;

            for (int i = 0, n = _left.Length; i < n; i++)
            {
                result &= _left[i].Equals(_right[i]);
            }

            return result;
        }
        public static string[] GetForeignKeyName(this PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            string[] result = propertyInfo
                                .GetCustomAttributes<ForeignKeyAttribute>()
                                .Select(attribute => attribute.Name)
                                .ToArray();

            return result.Length == 0 ? new[] { $"{propertyInfo.Name}ID" } : result;
        }

        public static string[] GetPrimaryKeyName<TEntity>()
            where TEntity : class
        {
            string[] result = typeof(TEntity)
                                .GetProperties()
                                .Where(property => property.GetCustomAttribute<KeyAttribute>().IsNotNull())
                                .Select(property => property.Name)
                                .ToArray();

            return result.Length == 0 ? new[] { $"ID" } : result;
        }

        public static TType GetValue<TType>(this PropertyInfo source, object value, object[] index = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (TType)source.GetValue(value, index);
        }

        public static SubSonicParameter Get(this IReadOnlyCollection<SubSonicParameter> parameters, string name)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (SubSonicParameter parameter in parameters)
            {
                if(parameter.ParameterName == name)
                {
                    return parameter;
                }
            }
            return null;
        }

        public static void Map<TDestination, TSource>(this TDestination destination, TSource source, Func<string, string> nameOf = null)
        {
            if(nameOf is null)
            {
                nameOf = (name) => name;
            }

            Type
                destinationType = typeof(TDestination),
                sourceType = typeof(TSource);

            foreach (PropertyInfo property in destinationType.GetProperties())
            {
                PropertyInfo sourceInfo = sourceType.GetProperty(nameOf(property.Name));

                if (sourceInfo.IsNotNull())
                {
                    object value = sourceInfo.GetValue(source);

                    if (!property.PropertyType.IsEnum)
                    {
                        if (!(value is null))
                        {
                            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                            {
                                property.SetValue(destination, value);
                            }
                            else
                            {
                                property.SetValue(destination, Convert.ChangeType(value, property.PropertyType, CultureInfo.CurrentCulture));
                            }
                        }
                    }
                    else
                    {
                        if (!(value is string))
                        {
                            value = Enum.GetName(property.PropertyType, value);
                        }

                        if (value != null)
                        {
                            if (Enum.TryParse(property.PropertyType, (string)value, out object @enum))
                            {
                                property.SetValue(destination, @enum);
                            }
                            else
                            {
                                throw new InvalidOperationException($"'{value}' is not found on {property.PropertyType.FullName}");
                            }
                        }
                        else
                        {
                            property.SetValue(destination, property.GetCustomAttribute<DefaultValueAttribute>().IsNotNull(Def => Def.Value, Activator.CreateInstance(property.PropertyType)));
                        }

                    }
                }
            }
        }
    }
}
