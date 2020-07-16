using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SubSonic
{
    using src;
    using Linq;
    using Data.DynamicProxies;
    using Schema;

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

        public static string[] GetForeignKeyName(this Type entityType)
        {
            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            string[] result = entityType
                                .GetProperties()
                                .SelectMany(property =>
                                    property.GetCustomAttributes<ForeignKeyAttribute>())
                                .Where(attribute => attribute.IsNotNull())
                                .Select(attribute => attribute.Name)
                                .ToArray();

            System.Diagnostics.Debug.Assert(result.Length > 0);

            return result;
        }

        public static string[] GetPrimaryKeyName<TEntity>()
            where TEntity : class
        {
            return GetPrimaryKeyName(typeof(TEntity));
        }

        public static string[] GetPrimaryKeyName(Type entityType)            
        {
            if (entityType is null)
            {
                throw Error.ArgumentNull(nameof(entityType));
            }

            string[] result = entityType
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <remarks>
        /// experimental concurency approach to loading data
        /// </remarks>
        public static IEnumerable<TEntity> ReadDataInParallel<TEntity>(this DbDataReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            ICollection<TEntity> entities = new List<TEntity>();

            Parallel.ForEach(reader.ParallelMap<TEntity>(), entity => entities.Add(entity));

            return entities;
        }

        public static IEnumerable<TEntity> ParallelMap<TEntity>(this DbDataReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.HasRows)
            {
                IDbEntityModel model = SubSonicContext.DbModel.GetEntityModel<TEntity>();

                while (reader.Read())
                {
                    yield return reader.Map<TEntity>(model);
                }
            }
        }

        public static TEntity Map<TEntity>(this DbDataReader reader, IDbEntityModel model)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            TEntity item = DynamicProxy.CreateProxyInstanceOf<TEntity>(SubSonicContext.Current);

            foreach (IDbEntityProperty property in model.Properties)
            //Parallel.ForEach(model.Properties, property =>
            {
                if (property.EntityPropertyType != DbEntityPropertyType.Value)
                {
                    continue;
                    //return;
                }

                int ordinal = reader.GetOrdinal(property.Name);

                object value = reader.GetValue(ordinal);

                if (value != DBNull.Value )
                {
                    if (value.IsOfType(property.PropertyType.GetUnderlyingType()))
                    {
                        model.EntityModelType.GetProperty(property.PropertyName).SetValue(item, value);
                    }
                    else
                    {
                        throw new InvalidOperationException(SubSonicErrorMessages.ValueIsNotOfExpectedType.Format(property.PropertyType.Name, value.GetType().Name));
                    }
                }
            }
            //);

            return item;
        }

        public static IEnumerable<TEntity> ReadData<TEntity>(this DbDataReader reader, Action<TEntity> callback)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (callback is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            ICollection<TEntity> result = new List<TEntity>();

            if (reader.HasRows)
            {
                IDbEntityModel model = SubSonicContext.DbModel.GetEntityModel<TEntity>();

                while (reader.Read())
                {
                    TEntity item = reader.Map<TEntity>(model);

                    if (callback.IsNotNull())
                    {
                        callback(item);
                    }

                    result.Add(item);
                }
            }

            reader.Close();

            return result;
        }

        public static IEnumerable<TEntity> ReadData<TEntity>(this DbDataReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            ICollection<TEntity> result = new List<TEntity>();

            if (reader.HasRows)
            {
                IDbEntityModel model = SubSonicContext.DbModel.GetEntityModel<TEntity>();

                while (reader.Read())
                {
                    TEntity item = reader.Map<TEntity>(model);

                    result.Add(item);
                }
            }

            reader.Close();

            return result;
        }
    }
}
