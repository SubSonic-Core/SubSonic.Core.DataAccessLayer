using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq
{
    using Expressions;
    using Alias = Expressions.Alias;
    using Infrastructure.Schema;
    using Infrastructure;

    public static partial class SubSonicExtensions
    {
        public static Alias.TableAlias ToAlias(this IDbEntityModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return new Alias.TableAlias(model.ToString());
        }

        public static IEnumerable<DbColumnDeclaration> ToColumnList(this ICollection<IDbEntityProperty> properties, DbAliasedExpression expression)
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
                        property.SetExpression(expression.Alias);

                        columns.Add(new DbColumnDeclaration(property.PropertyName, property.Order, property.Expression));
                    }
                }
            }

            return columns;
        }
    }
}
