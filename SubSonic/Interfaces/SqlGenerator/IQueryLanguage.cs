using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    public interface IQueryLanguage
    {
        string ClientName { get; set; }
        DbProviderFactory DataProvider { get; set; }
        string Quote(string name);
        bool IsScalar(Type type);
        bool CanBeColumn(Expression expression);
        Expression Translate(Expression expression);
        string Format(Expression expression);
        Expression Parameterize(Expression expression);
    }
}
