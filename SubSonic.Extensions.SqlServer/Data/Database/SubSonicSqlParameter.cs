using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using System.Data;

namespace SubSonic.Extensions.SqlServer
{
    public class SubSonicSqlParameter
        : SubSonicParameter
    {
        public SubSonicSqlParameter(string name, object value)
            : base(name, value)
        {
        }

        public SubSonicSqlParameter(string name, object value, IDbEntityProperty property)
            : base(name, value, property)
        {
        }

        public new SqlDbType DbType
        {
            get
            {
                return (SqlDbType)base.DbType;
            }
            set
            {
                DbType = value;
            }
        }
    }
}
