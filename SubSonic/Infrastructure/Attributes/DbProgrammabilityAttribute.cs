using System;
using System.Globalization;

namespace SubSonic.Infrastructure
{
    public abstract class DbProgrammabilityAttribute
        : Attribute
    {
        #region fields
        private string _schema = null;
        #endregion

        protected DbProgrammabilityAttribute(string name, DbProgrammabilityType dbProgrammabilityType)
            : base()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            Name = name;
            DbProgrammabilityType = dbProgrammabilityType;
        }

        #region properties
        public string Schema { get { return _schema ?? "dbo"; } set { _schema = value; } }

        public string Name { get; private set; }

        public DbProgrammabilityType DbProgrammabilityType { get; private set; }

        public Type[] ResultTypes { get; set; }
        #endregion

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}].[{1}]", Schema, Name);
        }
    }
}
