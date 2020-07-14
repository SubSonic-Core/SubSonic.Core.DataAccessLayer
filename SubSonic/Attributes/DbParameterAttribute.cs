using System;
using System.Data;

namespace SubSonic
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public abstract class DbParameterAttribute
        : DbProgrammabilityAttribute
    {
        private int _size;

        public DbParameterAttribute(string name)
            : base(name, DbProgrammabilityType.Parameter)
        {
        }

        public DbType DbType { get; set; }

        public ParameterDirection Direction { get; set; }

        public DbProgrammabilityOptions Options { get; set; }

        /// <summary>
        /// Size of the parameter, typically used for output parameters. If an output parameter has a size of 0 then its default to -1 (equivalent to MAX)
        /// </summary>
        public int Size { get { return Direction != ParameterDirection.Input && _size == 0 ? -1 : _size; } set { _size = value; } }
    }
}
