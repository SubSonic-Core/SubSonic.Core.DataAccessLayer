using SubSonic.Infrastructure.Schema;
using SubSonic.Linq.Expressions.Alias;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    public class DbTableTypeExpression
        : DbTableExpression
    {
        protected internal DbTableTypeExpression(IDbEntityModel model, string name)
            : base(model, new TableAlias(name ?? model.IsNullThrowArgumentNull(nameof(model)).Name))
        {
            Name = name;
        }

        public string Name { get; }

        public override string QualifiedName => $"{(Name ?? Model?.Name ?? Type.Name).ToLower(CultureInfo.CurrentCulture)}";
    }

    public partial class DbExpression
    {
        public static DbExpression DbTableType(IDbEntityModel entityModel, string name)
        {
            return new DbTableTypeExpression(entityModel, name);
        }
    }
}
