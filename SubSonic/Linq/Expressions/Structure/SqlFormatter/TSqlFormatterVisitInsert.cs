using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    public partial class TSqlFormatter
    {
        protected internal override Expression VisitInsert(DbInsertExpression insert)
        {
            if (insert.IsNotNull())
            {
                return base.VisitInsert(insert);
            }

            return null;
        }
    }
}
