using System.Data.Common;

namespace SubSonic
{
    public interface IConnectionScope
    {
        DbConnection Connection { get; }
    }
}
