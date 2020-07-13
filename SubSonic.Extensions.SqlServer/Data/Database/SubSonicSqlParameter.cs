using SubSonic;
using SubSonic.Schema;
using System.Data;

namespace SubSonic.Extensions.SqlServer
{
    public class SubSonicSqlParameter
        : SubSonicParameter
    {
        public SubSonicSqlParameter(string name, object value)
            : base(name, value)
        {
            SqlDbType = TypeConvertor.ToSqlDbType(value.GetType(), SubSonicContext.DbOptions.SupportUnicode);
        }

        public SubSonicSqlParameter(string name, object value, IDbEntityProperty property)
            : base(name, value, property)
        {
            SqlDbType = TypeConvertor.ToSqlDbType(property.DbType, SubSonicContext.DbOptions.SupportUnicode);
        }

        private SqlDbType sqlDbType;

        public SqlDbType SqlDbType
        {
            get
            {
                return sqlDbType;
            }
            set
            {
                sqlDbType = value;

                DbType = TypeConvertor.ToDbType(value, SubSonicContext.DbOptions.SupportUnicode);
            }
        }
    }
}
