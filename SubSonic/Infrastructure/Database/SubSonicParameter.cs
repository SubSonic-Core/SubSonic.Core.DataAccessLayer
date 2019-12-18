using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;

    public class SubSonicParameter
    {
        private readonly IDbEntityProperty property;

        public SubSonicParameter(IDbEntityProperty property, string name, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("", nameof(name));
            }

            this.property = property ?? throw new ArgumentNullException(nameof(property));
            ParameterName = name;
            Direction = direction;

            Initialize();
        }

        public int DbType { get; set; }
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

        protected virtual void OnInitialize()
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
    }
}
