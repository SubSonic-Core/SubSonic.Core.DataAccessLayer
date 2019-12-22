using System.Data;

namespace SubSonic.Infrastructure
{
    public class DbSqlParameterAttribute
        : DbParameterAttribute
    {
        public DbSqlParameterAttribute(string name) 
            : base(name)
        {
        }

        public SqlDbType SqlDbType
        {
            get
            {
                return (SqlDbType)DbType;
            }
            set
            {
                DbType = (int)value;
            }
        }
    }
}
