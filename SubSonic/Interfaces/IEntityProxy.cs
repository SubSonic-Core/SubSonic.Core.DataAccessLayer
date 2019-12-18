using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public interface IEntityProxy<TEntity>
        : IEntityProxy
    {
        TEntity Data { get; }
    }

    public interface IEntityProxy
    {
        IEnumerable<object> KeyData { get; }
        bool IsDirty { get; set; }
        bool IsNew { get; set; }

        void OnPropertyChange(IEntityProxy proxy);
    }
}
