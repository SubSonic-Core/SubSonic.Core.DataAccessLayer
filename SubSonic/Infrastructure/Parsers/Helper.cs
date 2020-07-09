using SubSonic.Infrastructure.Schema;
using SubSonic.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure.Parsers
{
    internal class Helper
    {
        public string GetParameterName(PropertyInfo propertyInfo)
        {
            var rValue = propertyInfo.Name;

            // grab the name of the parameter from the attribute, if it's defined.
            var attribute = propertyInfo.GetCustomAttribute<DbParameterAttribute>();
            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
                rValue = attribute.Name;

            return rValue;
        }

        public string StoreProcedureName(Type procedureType)
        {
            var attribute = procedureType.GetCustomAttribute<DbStoredProcedureAttribute>();

            if (attribute == null)
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    SubSonicErrorMessages.NotDecoratedWithStoredProcedureAttribute,
                    procedureType));

            return attribute.QualifiedName;
        }

        internal bool IsNonQuery(Type procedureType)
        {
            var attribute = procedureType.GetCustomAttribute<DbStoredProcedureAttribute>();

            return attribute.IsNonQuery;
        }

        public bool IsUserDefinedTableParameter(PropertyInfo propertyInfo)
        {
            Type collectionType = GetCollectionType(propertyInfo.PropertyType);

            return collectionType != null;
        }

        public bool ParameterIsMandatory(DbProgrammabilityOptions options)
        {
            return (options & DbProgrammabilityOptions.Mandatory) ==
                   DbProgrammabilityOptions.Mandatory;
        }

        public string GetUserDefinedTableType(PropertyInfo propertyInfo)
        {
            Type collectionType = GetCollectionType(propertyInfo.PropertyType);

            DbUserDefinedTableTypeAttribute attribute = collectionType.GetCustomAttribute<DbUserDefinedTableTypeAttribute>();

            if (attribute == null)
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SubSonicErrorMessages.UserDefinedTableNotDefined,
                        propertyInfo.PropertyType));

            return attribute.QualifiedName;
        }

        public string GetUserDefinedTableType(IDbEntityModel model)
        {
            return model.QualifiedName;
        }

        public object GetUserDefinedTableValue(PropertyInfo propertyInfo, object storedProcedure)
        {
            Type enumerableType = GetCollectionType(propertyInfo.PropertyType);

            var generator = new DbUserDefinedTableBuilder(enumerableType, (IEnumerable)propertyInfo.GetValue(storedProcedure));

            return generator.GenerateTable();
        }

        public object GetUserDefinedTableValue(IDbEntityModel model, PropertyInfo property, object storedProcedure)
        {
            var generator = new DbUserDefinedTableBuilder(model, (IEnumerable)property.GetValue(storedProcedure));

            return generator.GenerateTable();
        }

        public static Type GetCollectionType(Type type)
        {
            if (type.IsGenericType)
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if(interfaceType.NotIn(typeof(IEnumerable)))
                    {
                        continue;
                    }

                    return type.GetQualifiedType();
                }
            }
            else if (type.IsArray)
            {
                return type.GetElementType();
            }

            return null;
        }
    }
}
