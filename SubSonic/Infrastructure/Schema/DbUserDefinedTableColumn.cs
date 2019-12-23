using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public class DbUserDefinedTableColumn
    {
        public DbUserDefinedTableColumn(PropertyInfo info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            DbUserDefinedTableTypeColumnAttribute TableTypeColumn = null;

            Property = info;
            Name = TableTypeColumn.IsNotNull(x => x.Name) ?? Property.Name;
            Order = TableTypeColumn.IsNotNull(x => x.Order, -1);
            IsNullable = TableTypeColumn.IsNotNull(x => x.IsNullable, Property.PropertyType.IsNullableType());
            DbType = TableTypeColumn.IsNotNull(x => x.DbType, (int)Property.PropertyType.GetDbType());
            IsPrimaryKey = !(info.GetCustomAttribute<KeyAttribute>() is null);
        }

        public DbUserDefinedTableColumn(PropertyInfo info, IDbEntityProperty property)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            Property = info;
            Name = property.Name;
            Order = property.Order;
            IsNullable = property.IsNullable;
            DbType = (int)property.DbType;
            IsPrimaryKey = property.IsPrimaryKey;
        }

        public PropertyInfo Property { get; }
        public string Name { get; }
        public int Order { get; set; }
        public bool IsNullable { get; }
        public int DbType { get; }
        public bool IsPrimaryKey { get; }
    }
}
