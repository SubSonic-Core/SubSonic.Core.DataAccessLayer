using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace SubSonic.Extensions.Test.MockDbClient
{
    public class MockDbParameterCollection
        : DbParameterCollection
        , IList<MockDbParameter>
    {
        List<MockDbParameter> inner;
        object sync;

        internal MockDbParameterCollection()
        {
            inner = new List<MockDbParameter>();
            sync = new object();
        }
        public override int Add(object value)
        {
            inner.Add((MockDbParameter)value);
            return inner.Count;
        }

        public override void AddRange(Array values)
        {
            inner.AddRange(values.Cast<MockDbParameter>());
        }

        public override void Clear()
        {
            inner.Clear();
        }

        public override bool Contains(string value)
        {
            return inner.Any(c => c.ParameterName == value);
        }

        public override bool Contains(object value)
        {
            return inner.Any(c => c.Value == value);
        }

        public override void CopyTo(Array array, int index)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            Array.Copy(inner.ToArray(), index, array, 0, array.Length);
        }

        public override int Count
        {
            get { return inner.Count; }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return inner.SingleOrDefault(x => x.ParameterName == parameterName);
        }

        protected override DbParameter GetParameter(int index)
        {
            return inner[index];
        }

        public override int IndexOf(string parameterName)
        {
            return inner.IndexOf(inner.Single(x => x.ParameterName == parameterName));
        }

        public override int IndexOf(object value)
        {
            return inner.IndexOf(inner.Single(x => x.Value == value));
        }

        public override void Insert(int index, object value)
        {
            inner.Insert(index, (MockDbParameter)value);
        }

        public override bool IsFixedSize
        {
            get { return false; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsSynchronized
        {
            get { return false; }
        }

        public override void Remove(object value)
        {
            inner.Remove((MockDbParameter)value);
        }

        public override void RemoveAt(string parameterName)
        {
            inner.Remove(inner.Single(x => x.ParameterName == parameterName));
        }

        public override void RemoveAt(int index)
        {
            inner.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentException("", nameof(parameterName));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var p = inner.Single(x => x.ParameterName == parameterName);
            p.ParameterName = value.ParameterName;
            p.Value = value.Value;

        }

        protected override void SetParameter(int index, DbParameter value)
        {
            SetParameter(inner[index].ParameterName, value);
        }

        #region IList<> Implementation
        public int IndexOf(MockDbParameter item)
        {
            return ((IList<MockDbParameter>)inner).IndexOf(item);
        }

        public void Insert(int index, MockDbParameter item)
        {
            ((IList<MockDbParameter>)inner).Insert(index, item);
        }

        public void Add(MockDbParameter item)
        {
            ((IList<MockDbParameter>)inner).Add(item);
        }

        public bool Contains(MockDbParameter item)
        {
            return ((IList<MockDbParameter>)inner).Contains(item);
        }

        public void CopyTo(MockDbParameter[] array, int arrayIndex)
        {
            ((IList<MockDbParameter>)inner).CopyTo(array, arrayIndex);
        }

        public bool Remove(MockDbParameter item)
        {
            return ((IList<MockDbParameter>)inner).Remove(item);
        }

        IEnumerator<MockDbParameter> IEnumerable<MockDbParameter>.GetEnumerator()
        {
            return ((IList<MockDbParameter>)inner).GetEnumerator();
        }

        public override object SyncRoot
        {
            get { return sync; }
        }

        MockDbParameter IList<MockDbParameter>.this[int index] { get => ((IList<MockDbParameter>)inner)[index]; set => ((IList<MockDbParameter>)inner)[index] = value; }
        #endregion
    }
}