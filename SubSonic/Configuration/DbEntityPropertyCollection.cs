using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Schema;
    using System.Collections;

    public class DbEntityPropertyCollection
        : ICollection<IDbEntityProperty>

    {
        private readonly IDbEntityModel dbEntityModel;
        private readonly List<IDbEntityProperty> dbEntityProperties;

        public DbEntityPropertyCollection(IDbEntityModel dbEntityModel)
        {
            this.dbEntityModel = dbEntityModel ?? throw new ArgumentNullException(nameof(dbEntityModel));
            this.dbEntityProperties = new List<IDbEntityProperty>();
        }

        public int Count => dbEntityProperties.Count;

        public bool IsReadOnly => true;

        public void Add(IDbEntityProperty item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.EntityModel.Equals(dbEntityModel))
            {
                item.Order = dbEntityProperties.Count;
                dbEntityProperties.Add(item);
            }
        }

        public void Clear()
        {
            dbEntityProperties.Clear();
        }

        public bool Contains(IDbEntityProperty item)
        {
            return dbEntityProperties.Contains(item);
        }

        public void CopyTo(IDbEntityProperty[] array, int arrayIndex)
        {
            dbEntityProperties.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IDbEntityProperty> GetEnumerator()
        {
            return dbEntityProperties.GetEnumerator();
        }

        public bool Remove(IDbEntityProperty item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return dbEntityProperties.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dbEntityProperties).GetEnumerator();
        }
    }
}
