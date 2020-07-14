using System;
using System.Data.Common;
using System.Linq.Expressions;

namespace SubSonic.SqlGenerator
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
