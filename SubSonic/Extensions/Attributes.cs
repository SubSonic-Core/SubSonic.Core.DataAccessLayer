using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace SubSonic
{
    public static partial class Extensions
    {
        public static string GetForeignKeyName(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<ForeignKeyAttribute>().IsNotNull(Key => Key.Name, $"{propertyInfo.Name}ID");
        }

        public static string GetPrimaryKeyName<TEntity>(this TEntity entity)
        {
            PropertyInfo keyProperty = null;

            foreach(PropertyInfo property in typeof(TEntity).GetProperties())
            {
                if(property.GetCustomAttribute<KeyAttribute>().IsNotNull())
                {
                    keyProperty = property;
                    break;
                }
            }

            return keyProperty.IsNotNull(key => key.Name, "ID");
        }

        public static TType GetValue<TType>(this PropertyInfo source, object value, object[] index = null)
        {
            return (TType)source.GetValue(value, index);
        }

    }
}
