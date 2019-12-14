using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace SubSonic.Infrastructure
{
    public class SubSonicParameterCollection
        : ICollection<SubSonicParameter>
    {
        private readonly IList<SubSonicParameter> parameters;

        public SubSonicParameterCollection()
        {
            parameters = new List<SubSonicParameter>();
        }

        public int Count => parameters.Count;

        public bool IsReadOnly => parameters.IsReadOnly;

        public void Add(SubSonicParameter item)
        {
            parameters.Add(item);
        }

        public void Clear()
        {
            parameters.Clear();
        }

        public bool Contains(SubSonicParameter item)
        {
            return parameters.Contains(item);
        }

        public void CopyTo(SubSonicParameter[] array, int arrayIndex)
        {
            parameters.CopyTo(array, arrayIndex);
        }

        public IEnumerator<SubSonicParameter> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        public bool Remove(SubSonicParameter item)
        {
            return parameters.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)parameters).GetEnumerator();
        }

        public IReadOnlyCollection<SubSonicParameter> ToReadOnly()
        {
            return new ReadOnlyCollection<SubSonicParameter>(parameters);
        }
    }
}
