using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    using Linq.Expressions;
    using System.Data;

    public interface IDbEntityProperty
        : IDbObject

    {
        IDbEntityModel EntityModel { get; }

        string PropertyName { get; }
        Type PropertyType { get; }
        bool IsPrimaryKey { get; }
        IEnumerable<string> ForeignKeys { get; }
        int Size { get; }
        int Scale { get; }
        int Precision { get; }
        bool IsRequired { get; }
        bool IsNullable { get; }
        bool IsReadOnly { get; }
        bool IsComputed { get; }
        bool IsAutoIncrement { get; }
        int Order { get; set; }
        int DbType { get; set; }
        DbEntityPropertyType EntityPropertyType { get; }
        DbColumnExpression Expression { get; }
    }
}
