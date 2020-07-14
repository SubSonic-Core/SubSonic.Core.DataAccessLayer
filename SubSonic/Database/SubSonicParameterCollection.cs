using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace SubSonic
{
    using Linq;
    using Linq.Expressions;

    public class SubSonicParameterDictionary
        : IDictionary<DbExpressionType, List<DbParameter>>
    {
        private readonly IDictionary<DbExpressionType, List<DbParameter>> parameters;

        public SubSonicParameterDictionary()
        {
            parameters = new Dictionary<DbExpressionType, List<DbParameter>>();
        }

        public int Count => parameters.Sum(x => x.Value.Count);

        public bool IsReadOnly => parameters.IsReadOnly;

        public List<DbParameter> this[DbExpressionType key] { get => parameters.ContainsKey(key) ? parameters[key] : null; set => parameters[key] = value;}

        public ICollection<DbExpressionType> Keys => parameters.Keys;

        public ICollection<List<DbParameter>> Values => parameters.Values;

        public void AddRange(DbExpressionType key, params DbParameter[] parameters)
        {
            if (this.parameters.ContainsKey(key))
            {
                this.parameters[key].AddRange(parameters);
            }
            else
            {
                Add(new KeyValuePair<DbExpressionType, List<DbParameter>>(key, new List<DbParameter>(parameters)));
            }
        }

        public void Add(DbExpressionType key, DbParameter parameter)
        {
            if(parameters.ContainsKey(key))
            {
                parameters[key].Add(parameter);
            }
            else
            {
                Add(new KeyValuePair<DbExpressionType, List<DbParameter>>(key, new List<DbParameter>() { parameter }));
            }
        }

        public void Add(DbExpressionType key, List<DbParameter> list) => parameters.Add(key, list);

        public void Add(KeyValuePair<DbExpressionType, List<DbParameter>> item) => parameters.Add(item);

        public bool TryGetValue(DbExpressionType key, out List<DbParameter> parameters) => (parameters = this.parameters[key]).IsNotNull();

        public void Clear() => parameters.Clear();

        public bool ContainsKey(DbExpressionType key) => parameters.ContainsKey(key);

        public bool Contains(KeyValuePair<DbExpressionType, List<DbParameter>> keyValuePair) => parameters.Contains(keyValuePair);

        public void CopyTo(KeyValuePair<DbExpressionType, List<DbParameter>>[] array, int arrayIndex) => parameters.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<DbExpressionType, List<DbParameter>>> GetEnumerator() => parameters.GetEnumerator();

        public bool Remove(DbExpressionType key) => parameters.Remove(key);

        public bool Remove(KeyValuePair<DbExpressionType, List<DbParameter>> item) => parameters.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)parameters).GetEnumerator();

        public IReadOnlyCollection<DbParameter> ToReadOnlyCollection(DbExpressionType key) => new ReadOnlyCollection<DbParameter>(parameters[key]);
    }
}
