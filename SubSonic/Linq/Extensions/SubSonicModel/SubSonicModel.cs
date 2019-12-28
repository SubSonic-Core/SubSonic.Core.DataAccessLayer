using System;
using System.Collections.Generic;

namespace SubSonic.Linq
{
    using Expressions;
    using Infrastructure;
    using Infrastructure.Schema;
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

        public static IEnumerable<DbColumnDeclaration> ToColumnList(this ICollection<IDbEntityProperty> properties, DbTableExpression expression)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            ICollection<DbColumnDeclaration> columns = new List<DbColumnDeclaration>();

            if (properties.IsNotNull())
            {
                foreach (DbEntityProperty property in properties)
                {
                    if (property.EntityPropertyType == DbEntityPropertyType.Value)
                    {
                        property.SetExpression(expression.Table);

                        columns.Add(new DbColumnDeclaration(property));
                    }
                }
            }

            return columns;
        }
    }
}
