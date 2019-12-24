using System;
using System.Globalization;

namespace SubSonic.Infrastructure
{
    using Schema;

    public abstract class DbProgrammabilityAttribute
        : Attribute
        , IDbObject
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
        public string SchemaName { get { return _schema ?? "dbo"; } set { _schema = value; } }

        public string Name { get; private set; }

        public DbProgrammabilityType DbProgrammabilityType { get; private set; }

        public Type[] ResultTypes { get; set; }

        public string FriendlyName => Name;

        public string QualifiedName => $"{SchemaName}.{Name}".EncapsulateQualifiedName();
        #endregion

        public override string ToString() => QualifiedName;
    }
}
