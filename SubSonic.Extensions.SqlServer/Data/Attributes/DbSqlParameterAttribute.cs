using SubSonic.Infrastructure;
using System.Data;

namespace SubSonic.Extensions.SqlServer
{
    public class DbSqlParameterAttribute
        : DbParameterAttribute
    {
        public DbSqlParameterAttribute(string name)
            : base(name)
        {
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

                DbType = TypeConvertor.ToDbType(value);
            }
        }
    }
}
