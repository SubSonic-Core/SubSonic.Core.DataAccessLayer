using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbEntityProperty
    {
        public DbEntityProperty(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("message", nameof(columnName));
            }

            ColumnName = columnName;
        }

        public string ColumnName { get; }

        public string RuntimeName { get; internal set; }

        public bool IsKey { get; internal set; }

        public bool IsRequired { get; internal set; }

        public Type Type { get; internal set; }

        public EnumDbEntityPropertyType PropertyType
        {
            get
            {
                EnumDbEntityPropertyType result;
                if (Type.GetUnderlyingType().IsValueType)
                {
                    result = EnumDbEntityPropertyType.Value;
                }
                else if (Type.IsClass)
                {
                    result = EnumDbEntityPropertyType.Navigation;
                }
                else if(Type.GetGenericTypeDefinition() == typeof(ICollection<>) || Type.GetInterface(typeof(IEnumerable<>).Name).IsNotNull())
                {
                    result = EnumDbEntityPropertyType.Collection;
                }
                else
                {
                    throw new NotSupportedException($"Property Type \"{Type.GetQualifiedTypeName()}\", is not supported.");
                }

                return result;
            }
        }

        public override string ToString()
        {
            return RuntimeName;
        }
    }
}
