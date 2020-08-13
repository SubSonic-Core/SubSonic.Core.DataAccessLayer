using SubSonic.Attributes;
using SubSonic.Linq.Expressions;
using SubSonic.Linq.Expressions.Alias;
using SubSonic.Schema;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Linq.Expressions
{
    public class DbViewExpression
        : DbTableExpression
    {
        protected internal DbViewExpression(IDbEntityModel model, TableAlias alias)
            : base(model, alias)
        {
        }

        public bool IsQuery => Query.IsNotNullOrEmpty();

        public string Query
        {
            get
            {
                if (Model.EntityModelType.GetCustomAttribute<DbViewAttribute>() is DbViewAttribute dbView)
                {
                    return dbView.Query;
                }
                return default;
            }
        }
    }

    public partial class DbExpression
    {
        public static DbExpression DbView(IDbEntityModel model, TableAlias alias)
        {
            return new DbViewExpression(model, alias);
        }
    }
}
