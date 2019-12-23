using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace SubSonic.Infrastructure
{
    using Linq.Expressions;
    using Linq;

    public class SubSonicParameterDictionary
        : IDictionary<DbExpressionType, List<SubSonicParameter>>
    {
        private readonly IDictionary<DbExpressionType, List<SubSonicParameter>> parameters;

        public SubSonicParameterDictionary()
        {
            parameters = new Dictionary<DbExpressionType, List<SubSonicParameter>>();
        }

        public int Count => parameters.Sum(x => x.Value.Count);

        public bool IsReadOnly => parameters.IsReadOnly;

        public List<SubSonicParameter> this[DbExpressionType key] { get => parameters.ContainsKey(key) ? parameters[key] : null; set => parameters[key] = value;}

        public ICollection<DbExpressionType> Keys => parameters.Keys;

        public ICollection<List<SubSonicParameter>> Values => parameters.Values;

        public void AddRange(DbExpressionType key, params SubSonicParameter[] parameters)
        {
            if (this.parameters.ContainsKey(key))
            {
                this.parameters[key].AddRange(parameters);
            }
            else
            {
                Add(new KeyValuePair<DbExpressionType, List<SubSonicParameter>>(key, new List<SubSonicParameter>(parameters)));
            }
        }

        public void Add(DbExpressionType key, SubSonicParameter parameter)
        {
            if(parameters.ContainsKey(key))
            {
                parameters[key].Add(parameter);
            }
            else
            {
                Add(new KeyValuePair<DbExpressionType, List<SubSonicParameter>>(key, new List<SubSonicParameter>() { parameter }));
            }
        }

        public void Add(DbExpressionType key, List<SubSonicParameter> list) => parameters.Add(key, list);

        public void Add(KeyValuePair<DbExpressionType, List<SubSonicParameter>> item) => parameters.Add(item);

        public bool TryGetValue(DbExpressionType key, out List<SubSonicParameter> parameters) => (parameters = this.parameters[key]).IsNotNull();

        public void Clear() => parameters.Clear();

        public bool ContainsKey(DbExpressionType key) => parameters.ContainsKey(key);

        public bool Contains(KeyValuePair<DbExpressionType, List<SubSonicParameter>> keyValuePair) => parameters.Contains(keyValuePair);

        public void CopyTo(KeyValuePair<DbExpressionType, List<SubSonicParameter>>[] array, int arrayIndex) => parameters.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<DbExpressionType, List<SubSonicParameter>>> GetEnumerator() => parameters.GetEnumerator();

        public bool Remove(DbExpressionType key) => parameters.Remove(key);

        public bool Remove(KeyValuePair<DbExpressionType, List<SubSonicParameter>> item) => parameters.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)parameters).GetEnumerator();

        public IReadOnlyCollection<SubSonicParameter> ToReadOnlyCollection(DbExpressionType key) => new ReadOnlyCollection<SubSonicParameter>(parameters[key]);
    }
}
