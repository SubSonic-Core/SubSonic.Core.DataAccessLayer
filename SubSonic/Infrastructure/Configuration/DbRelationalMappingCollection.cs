using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    using Schema;
    using System.Collections;

    public class DbRelationalMappingCollection
        : ICollection<IDbRelationalMapping>
    {
        private readonly IDbEntityModel dbEntityModel;
        private readonly List<IDbRelationalMapping> dbRelationalMappings;

        public DbRelationalMappingCollection(IDbEntityModel dbEntityModel)
        {
            this.dbEntityModel = dbEntityModel ?? throw new ArgumentNullException(nameof(dbEntityModel));
            this.dbRelationalMappings = new List<IDbRelationalMapping>();
        }

        public int Count => dbRelationalMappings.Count;

        public bool IsReadOnly => true;

        public void Add(IDbRelationalMapping item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.EntityModel.Equals(dbEntityModel))
            {
                dbRelationalMappings.Add(item);
            }
        }

        public void Clear()
        {
            dbRelationalMappings.Clear();
        }

        public bool Contains(IDbRelationalMapping item)
        {
            return dbRelationalMappings.Contains(item);
        }

        public void CopyTo(IDbRelationalMapping[] array, int arrayIndex)
        {
            dbRelationalMappings.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IDbRelationalMapping> GetEnumerator()
        {
            return dbRelationalMappings.GetEnumerator();
        }

        public bool Remove(IDbRelationalMapping item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return dbRelationalMappings.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dbRelationalMappings).GetEnumerator();
        }
    }
}
