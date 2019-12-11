using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public interface IDbEntityProperty
        : IDbObject
    {
        IDbEntityModel EntityModel { get; }

        string PropertyName { get; }
        Type PropertyType { get; }
        bool IsPrimaryKey { get; }
        IEnumerable<string> ForeignKeys { get; }
        int MaxLength { get; }
        int NumericScale { get; }
        int NumericPrecision { get; }
        bool IsRequired { get; }
        bool IsNullable { get; }
        bool IsReadOnly { get; }
        bool IsComputed { get; }
        bool IsAutoIncrement { get; }
        DbEntityPropertyType EntityPropertyType { get; }   
    }
}
