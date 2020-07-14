using SubSonic;
using SubSonic.Schema;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected internal override Expression VisitDelete(DbDeleteExpression delete)
        {
            if (delete.IsNotNull())
            {
                Write($"{Fragments.DELETE_FROM} {delete.From.QualifiedName}");

                Visit(delete.Where);
            }

            return delete;
        }
    }
}
