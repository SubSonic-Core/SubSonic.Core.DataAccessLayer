﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public class DbEntityProperty
        : DbObject
        , IDbEntityProperty
    {
        private readonly IDbEntityModel dbEntityModel;

        public DbEntityProperty(IDbEntityModel dbEntityModel, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("", nameof(columnName));
            }

            this.dbEntityModel = dbEntityModel ?? throw new ArgumentNullException(nameof(dbEntityModel));
            this.Name = columnName;
        }

        public IDbEntityModel EntityModel => dbEntityModel;

        public string PropertyName { get; internal set; }

        public Type PropertyType { get; internal set; }

        public bool IsPrimaryKey { get; internal set; }

        public IEnumerable<string> ForeignKeys { get; internal set;}

        public int MaxLength { get; internal set; }
        public int NumericScale { get; internal set; }
        public int NumericPrecision { get; internal set; }
        public bool IsRequired { get; internal set; }
        public bool IsNullable { get; internal set; }
        public bool IsReadOnly { get; internal set; }
        public bool IsComputed { get; internal set; }
        public bool IsAutoIncrement { get; internal set; }

        public EnumDbEntityPropertyType EntityPropertyType
        {
            get
            {
                EnumDbEntityPropertyType result;
                if (PropertyType.GetUnderlyingType().IsValueType)
                {
                    result = EnumDbEntityPropertyType.Value;
                }
                else if (PropertyType.IsClass)
                {
                    result = EnumDbEntityPropertyType.Navigation;
                }
                else if(PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) || PropertyType.GetInterface(typeof(IEnumerable<>).Name).IsNotNull())
                {
                    result = EnumDbEntityPropertyType.Collection;
                }
                else
                {
                    throw new NotSupportedException($"Property Type \"{PropertyType.GetQualifiedTypeName()}\", is not supported.");
                }

                return result;
            }
        }

        public override string ToString()
        {
            return PropertyName;
        }
    }
}