using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;
    using Linq;

    public class SubSonicParameter
        : IDbDataParameter
    {
        private readonly IDbEntityProperty property;

        public SubSonicParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            ParameterName = name;
            Value = value;
            Direction = ParameterDirection.Input;

            Initialize();
        }

        public SubSonicParameter(string name, object value, IDbEntityProperty property)
            : this(name, value)
        {
            this.property = property ?? throw new ArgumentNullException(nameof(property));

            Initialize();
        }

        public virtual DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public string ParameterName { get; set; }
        public string SourceColumn { get; set; }
        public DataRowVersion SourceVersion { get; set; }
        public object Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }

        protected void Initialize()
        {
            OnInitialize();
        }

        protected virtual DbType DetermineDbType()
        {
            if(Value is null)
            {
                throw new InvalidOperationException();
            }

            return Value.GetType().GetDbType(DbContext.DbOptions.SupportUnicode);
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
