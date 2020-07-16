using System.Data;
using System.Reflection;

namespace SubSonic
{
    internal class DbStoredProcedureParameter
    {
        public ParameterDirection Direction { get; set; }
        public string Name { get; set; }
        public DbType DbType { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsUserDefinedTable { get; set; }
        public int Size { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}
