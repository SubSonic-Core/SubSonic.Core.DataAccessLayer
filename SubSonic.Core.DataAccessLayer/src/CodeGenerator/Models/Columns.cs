using SubSonic.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;

namespace SubSonic.CodeGenerator.Models
{
    [DbView(Query = Query)]
    public class Column
    {
        public const string Query =
@"SELECT 
    [Catalog]			= [COL].[TABLE_CATALOG],
    [Schema]			= [COL].[TABLE_SCHEMA], 
    [TableName]			= [COL].[TABLE_NAME], 
    [ColumnName]		= [COL].[COLUMN_NAME], 
    [OrdinalPosition]	= [COL].[ORDINAL_POSITION], 
    [DefaultValue]		= [COL].[COLUMN_DEFAULT], 
    [IsNullable]		= CASE WHEN [COL].[IS_NULLABLE] = 'YES' THEN 1 ELSE 0 END,
	[DataType]			= [COL].[DATA_TYPE], 
    [MaximumLength]		= [COL].[CHARACTER_MAXIMUM_LENGTH], 
	[NumericPrecision]	= [COL].[NUMERIC_PRECISION],
	[NumericScale]		= [COL].[NUMERIC_SCALE],
    [DatePrecision]		= [COL].[DATETIME_PRECISION],
	[IsPrimaryKey]		= CASE WHEN [PK].COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END,
    [IsIdentity]		= COLUMNPROPERTY(object_id('[' + [COL].TABLE_SCHEMA + '].[' + [COL].TABLE_NAME + ']'), [COL].COLUMN_NAME, 'IsIdentity'),
    [IsComputed]		= COLUMNPROPERTY(object_id('[' + [COL].TABLE_SCHEMA + '].[' + [COL].TABLE_NAME + ']'), [COL].COLUMN_NAME, 'IsComputed')
FROM  INFORMATION_SCHEMA.COLUMNS COL
	LEFT JOIN ( SELECT KCU.TABLE_SCHEMA, KCU.TABLE_NAME, KCU.ORDINAL_POSITION, KCU.COLUMN_NAME
				FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
					JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
					ON KCU.CONSTRAINT_NAME = TC.CONSTRAINT_NAME
				WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND KCU.TABLE_NAME <> '__RefactorLog') [PK] ON
					[PK].TABLE_SCHEMA = COL.TABLE_SCHEMA AND 
					[PK].TABLE_NAME = [COL].[TABLE_NAME] AND
					[PK].ORDINAL_POSITION = [COL].ORDINAL_POSITION
WHERE [COL].[TABLE_NAME] <> '__RefactorLog'";

        public string Catalog { get; set; }
        public string Schema { get; set; }
        [Key]
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        [Key]
        public int OrdinalPosition { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
        public string DataType { get; set; }
        public int? MaximumLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public decimal? DatePrecision { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }

        [ForeignKey(nameof(TableName))]
        public virtual Table Table { get; set; }

        public SqlDbType GetSqlDbType()
        {
            if (Enum.TryParse(DataType, true, out SqlDbType dbType))
            {
                return dbType;
            }
            return default;
        }

        public DbType GetDbType()
        {
            return TypeConvertor.ToDbType(GetSqlDbType());
        }

        public Type GetClrType()
        {
            return TypeConvertor.ToNetType(GetSqlDbType());
        }

        public override string ToString()
        {
            return $"[{TableName}].[{ColumnName}]";
        }
    }
}
