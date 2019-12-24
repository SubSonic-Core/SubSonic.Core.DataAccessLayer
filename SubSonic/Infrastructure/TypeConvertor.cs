//Convert .Net Type to SqlDbType or DbType and vise versa
//This class can be useful when you make conversion between types .The class supports conversion between .Net Type , SqlDbType and DbType .

namespace SubSonic.Infrastructure
{
    using System;
    using System.Collections;
    using System.Data;

    /// <summary>
    /// Convert a base data type to another base data type
    /// </summary>
    public sealed class TypeConvertor
    {

        private struct DbTypeMapEntry
        {
            public Type Type;
            public DbType DbType;
            public DbType? UnicodeDbType;
            public SqlDbType SqlDbType;
            public SqlDbType? UnicodeSqlDbType;

            public DbTypeMapEntry(Type type, DbType dbType, DbType? unicodeDbType, SqlDbType sqlDbType, SqlDbType? unicodeSqlDbType)
            {
                Type = type;
                DbType = dbType;
                UnicodeDbType = unicodeDbType;
                SqlDbType = sqlDbType;
                UnicodeSqlDbType = unicodeSqlDbType;
            }

            public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType)
                : this(type, dbType, null, sqlDbType, null) { }

        };

        private static ArrayList _DbTypeList = new ArrayList();

        #region Constructors

        static TypeConvertor()
        {
            DbTypeMapEntry dbTypeMapEntry
            = new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(decimal), DbType.Decimal, SqlDbType.Decimal);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(short), DbType.Int16, SqlDbType.SmallInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(int), DbType.Int32, SqlDbType.Int);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(long), DbType.Int64, SqlDbType.BigInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(string), DbType.AnsiString, DbType.String, SqlDbType.VarChar, SqlDbType.NVarChar);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(DataTable), DbType.Object, SqlDbType.Structured);
            _DbTypeList.Add(dbTypeMapEntry);
        }

        private TypeConvertor()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert db type to .Net data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type ToNetType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert TSQL type to .Net data type
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static Type ToNetType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert .Net type to Db type
        /// </summary>
        /// <param name="netType"></param>
        /// <returns></returns>
        public static DbType ToDbType(Type netType, bool unicode = false)
        {
            DbTypeMapEntry entry = Find(netType);
            return unicode ? entry.UnicodeDbType.GetValueOrDefault(entry.DbType) : entry.DbType;
        }

        /// <summary>
        /// Convert TSQL data type to DbType
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static DbType ToDbType(SqlDbType sqlDbType, bool unicode = false)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return unicode ? entry.UnicodeDbType.GetValueOrDefault(entry.DbType) : entry.DbType;
        }

        /// <summary>
        /// Convert .Net type to TSQL data type
        /// </summary>
        /// <param name="netType"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(Type netType, bool unicode = false)
        {
            DbTypeMapEntry entry = Find(netType);
            return unicode ? entry.UnicodeSqlDbType.GetValueOrDefault(entry.SqlDbType) : entry.SqlDbType;
        }

        /// <summary>
        /// Convert DbType type to TSQL data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(DbType dbType, bool unicode = false)
        {
            DbTypeMapEntry entry = Find(dbType);
            return unicode ? entry.UnicodeSqlDbType.GetValueOrDefault(entry.SqlDbType) : entry.SqlDbType;
        }

        private static DbTypeMapEntry Find(Type type)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.Type == (Nullable.GetUnderlyingType(type) ?? type))
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                retObj = new DbTypeMapEntry(type, DbType.Object, SqlDbType.Variant);
            }

            return (DbTypeMapEntry)retObj;
        }

        private static DbTypeMapEntry Find(DbType dbType)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.DbType == dbType)
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                retObj = new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
            }

            return (DbTypeMapEntry)retObj;
        }
        private static DbTypeMapEntry Find(SqlDbType sqlDbType)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.SqlDbType == sqlDbType)
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                retObj = new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
            }

            return (DbTypeMapEntry)retObj;
        }

        #endregion
    }
}