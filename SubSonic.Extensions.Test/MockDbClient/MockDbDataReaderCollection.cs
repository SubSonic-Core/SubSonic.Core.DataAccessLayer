using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Collections;

namespace SubSonic.Extensions.Test.MockDbClient
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "<Pending>")]
    public class MockDbDataReaderCollection
        : DbDataReader
    {
        private readonly DbDataReader inner;
        public MockDbDataReaderCollection(DbDataReader inner)
        {
            this.inner = inner;
        }

        public MockDbDataReaderCollection()
        {
        }

        public override void Close()
        {
            inner.IsNotNull(x => x.Close());
        }

        public override int Depth
        {
            get { return inner.Depth; }
        }

        public override int FieldCount
        {
            get { return inner.FieldCount; }
        }

        public override bool GetBoolean(int ordinal)
        {
            return inner.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return inner.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return inner.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return inner.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return inner.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return inner.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return inner.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return inner.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return GetDouble(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            return inner.GetFieldType(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return inner.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return inner.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return inner.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return inner.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return inner.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return inner.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return inner.GetOrdinal(name);
        }

        public override DataTable GetSchemaTable()
        {
            return inner.GetSchemaTable();
        }

        public override string GetString(int ordinal)
        {
            return inner.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return inner.GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return inner.GetValues(values);
        }

        public override bool HasRows
        {
            get { return inner?.HasRows ?? false; }
        }

        public override bool IsClosed
        {
            get { return inner.IsClosed; }
        }

        public override bool IsDBNull(int ordinal)
        {
            return inner.IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            return inner.NextResult();
        }

        public override bool Read()
        {
            return inner.IsNotNull(x => x.Read());
        }

        public override int RecordsAffected
        {
            get { return inner.RecordsAffected; }
        }

        public override object this[string name]
        {
            get { return inner[name]; }
        }

        public override object this[int ordinal]
        {
            get { return inner[ordinal]; }
        }
    }
}