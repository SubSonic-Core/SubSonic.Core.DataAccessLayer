using System;
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
        public bool IsKey { get; internal set; }
    }
}
