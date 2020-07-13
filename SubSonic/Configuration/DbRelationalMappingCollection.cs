using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Schema;
    using System.Collections;

    public class DbRelationshipMapCollection
        : ICollection<IDbRelationshipMap>
    {
        private readonly IDbEntityModel dbEntityModel;
        private readonly List<IDbRelationshipMap> relationshipMaps;

        public DbRelationshipMapCollection(IDbEntityModel dbEntityModel)
        {
            this.dbEntityModel = dbEntityModel ?? throw new ArgumentNullException(nameof(dbEntityModel));
            this.relationshipMaps = new List<IDbRelationshipMap>();
        }

        public int Count => relationshipMaps.Count;

        public bool IsReadOnly => true;

        public void Add(IDbRelationshipMap item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            relationshipMaps.Add(item);
        }

        public void Clear()
        {
            relationshipMaps.Clear();
        }

        public bool Contains(IDbRelationshipMap item)
        {
            return relationshipMaps.Contains(item);
        }

        public void CopyTo(IDbRelationshipMap[] array, int arrayIndex)
        {
            relationshipMaps.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IDbRelationshipMap> GetEnumerator()
        {
            return relationshipMaps.GetEnumerator();
        }

        public bool Remove(IDbRelationshipMap item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return relationshipMaps.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)relationshipMaps).GetEnumerator();
        }
    }
}
