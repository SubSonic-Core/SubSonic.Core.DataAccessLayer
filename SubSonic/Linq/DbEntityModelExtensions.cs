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
        public static Alias.Table ToAlias(this IDbEntityModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return new Alias.Table(model.ToString());
        }

        public static IEnumerable<DbColumnDeclaration> ToColumnList(this ICollection<IDbEntityProperty> properties)
        {
            ICollection<DbColumnDeclaration> columns = new List<DbColumnDeclaration>();

            return columns;
        }
    }
}
