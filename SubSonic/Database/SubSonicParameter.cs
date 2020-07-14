using SubSonic.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;

namespace SubSonic
{
    using Schema;
    using Linq;

    public class SubSonicParameter
        : DbParameter
    {
        private readonly IDbEntityProperty property;

        public SubSonicParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            ParameterName = name;
            Value = value ?? throw new ArgumentNullException(nameof(value));

            Initialize();
        }

        public SubSonicParameter(string name, object value, IDbEntityProperty property)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            ParameterName = name;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            this.property = property ?? throw new ArgumentNullException(nameof(property));

            Initialize();
        }

        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; }
        public override string SourceColumn { get; set; }
        public override DataRowVersion SourceVersion { get; set; }
        public override object Value { get; set; }
        public override byte Precision { get; set; }
        public override byte Scale { get; set; }
        public override int Size { get; set; }

        public override void ResetDbType()
        {
            DbType = TypeConvertor.ToDbType(Value.GetType());
        }

        public override object InitializeLifetimeService()
        {
            return base.InitializeLifetimeService();
        }

        public override bool SourceColumnNullMapping { get; set; }

        protected void Initialize()
        {
            Direction = ParameterDirection.Input;
            SourceColumnNullMapping = IsNullable;

            OnInitialize();
        }

        protected virtual DbType DetermineDbType()
        {
            if(Value is null)
            {
                throw new InvalidOperationException();
            }

            return Value.GetType().GetDbType(SubSonicContext.DbOptions.SupportUnicode);
        }

        protected virtual void OnInitialize()
        {
            if (property.IsNotNull())
            {
                this.Map(property);
                this.Map<SubSonicParameter, IDbObject>(property, (dst) =>
                {
                    switch (dst)
                    {
                        case nameof(SourceColumn):
                            return nameof(property.Name);
                        default:
                            return dst;
                    }
                });
            }
            else
            {
                DbType = DetermineDbType();
            }
        }

        public override string ToString()
        {
            return $"{ParameterName} = {Value}";
        }
    }
}
