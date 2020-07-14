using System;
using System.Collections.Generic;

namespace SubSonic.Linq
{
    using Expressions;
    using Schema;
    using Alias = Expressions.Alias;

    public static partial class SubSonicModel
    {
        public static Alias.TableAlias ToAlias(this IDbEntityModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return new Alias.TableAlias(model.ToString());
        }

        public static IEnumerable<DbColumnDeclaration> ToColumnList(this ICollection<IDbEntityProperty> properties, DbTableExpression table)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            ICollection<DbColumnDeclaration> columns = new List<DbColumnDeclaration>();

            if (properties.IsNotNull())
            {
                foreach (DbEntityProperty property in properties)
                {
                    if (property.EntityPropertyType == DbEntityPropertyType.Value)
                    {
                        property.SetExpression(table.IsNamedAlias ? table.Alias.SetTable(table, true) : table.Alias);

                        columns.Add(new DbColumnDeclaration(property));
                    }
                }
            }

            return columns;
        }
    }
}
