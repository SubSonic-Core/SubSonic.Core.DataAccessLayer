using System;
using System.Collections.Generic;

namespace SubSonic
{
    public interface IEntityProxy<TEntity>
        : IEntityProxy
    {
        /// <summary>
        /// Get the actual data for the entity
        /// </summary>
        TEntity Data { get; }
        /// <summary>
        /// Iterate the properties of a model and update db computed properties.
        /// </summary>
        /// <param name="entity"></param>
        void SetDbComputedProperties(IEntityProxy<TEntity> fromDb);
        /// <summary>
        /// Iterate the properties of a model and ensure that the navigation property keys are current.
        /// </summary>
        void EnsureForeignKeys();
    }

    public interface IEntityProxy
    {
        IEnumerable<object> KeyData { get; }
        Type ModelType { get; }
        bool IsDirty { get; set; }
        bool IsNew { get; set; }
        bool IsDeleted { get; set; }

        void SetKeyData(IEnumerable<object> keyData);
    }
}
