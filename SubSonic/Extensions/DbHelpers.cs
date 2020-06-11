using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Data.Common;

namespace SubSonic
{
    using Linq;
    using Data.DynamicProxies;
    using Infrastructure;
    using Infrastructure.Schema;
    

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

        public static DbParameter Get(this IReadOnlyCollection<DbParameter> parameters, string name)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (DbParameter parameter in parameters)
            {
                if(parameter.ParameterName == name)
                {
                    return parameter;
                }
            }
            return null;
        }

        public static void ApplyOutputParameters(this DbParameterCollection parameters, object procedure)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (procedure is null)
            {
                throw new ArgumentNullException(nameof(procedure));
            }

            Type procedureType = procedure.GetType();

            foreach (DbParameter parameter in parameters)
            {
                if (parameter.Direction.In(ParameterDirection.Input) ||
                    parameter.Value == DBNull.Value)
                {
                    continue;
                }

                procedureType.GetProperty(parameter.ParameterName.TrimStart('@')).IsNotNull(info => info.SetValue(procedure, parameter.Value));
            }
        }

        public static TResult GetOutputParameter<TResult>(this DbParameterCollection parameters, string name)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (parameters[$"@{name}"].IsNotNull())
            {
                object value = parameters[$"@{name}"].Value;

                return value is DBNull ? default(TResult) : (TResult)parameters[$"@{name}"].Value;
            }

            return default(TResult);
        }

        public static void Map<TDestination, TSource>(this TDestination destination, TSource source, Func<string, string> nameOf = null)
        {
            if(nameOf is null)
            {
                nameOf = (name) => name;
            }

            Type
                destinationType = destination.GetType(),
                sourceType = source.GetType();

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
                        if (value != null)
                        {
                            if (Enum.IsDefined(property.PropertyType, value))
                            {
                                property.SetValue(destination, value);
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

        public static IEnumerable<TEntity> Map<TEntity>(this DbDataReader reader, Action<TEntity> callback = null)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            List<TEntity> result = new List<TEntity>();

            IDbEntityModel model = DbContext.DbModel.GetEntityModel<TEntity>();

            while (reader.Read())
            {
                TEntity item = DynamicProxy.CreateProxyInstanceOf<TEntity>(DbContext.Current);

                foreach(IDbEntityProperty property in model.Properties)
                {
                    if (property.EntityPropertyType != DbEntityPropertyType.Value)
                    {
                        continue;
                    }

                    if (reader[property.Name] != DBNull.Value)
                    {
                        model.EntityModelType.GetProperty(property.PropertyName).SetValue(item, reader[property.Name]);
                    }
                }

                if (callback.IsNotNull())
                {
                    callback(item);
                }

                result.Add(item);
            }

            reader.Close();

            return result;
        }
    }
}
