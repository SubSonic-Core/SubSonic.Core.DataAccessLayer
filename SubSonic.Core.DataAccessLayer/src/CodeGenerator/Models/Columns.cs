using SubSonic.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.CodeGenerator.Models
{
    [DbView(Query = Query)]
    public class Column
    {
        public const string Query =
@"SELECT 
    [Catalog]			= [TABLE_CATALOG],
    [Schema]			= [TABLE_SCHEMA], 
    [TableName]			= [TABLE_NAME], 
    [ColumnName]		= [COLUMN_NAME], 
    [OrdinalPosition]	= [ORDINAL_POSITION], 
    [DefaultValue]		= [COLUMN_DEFAULT], 
    [IsNullable]		= CASE WHEN [IS_NULLABLE] = 'YES' THEN 1 ELSE 0 END,
	[DataType]			= [DATA_TYPE], 
    [MaximumLength]		= [CHARACTER_MAXIMUM_LENGTH], 
	[NumericPrecision]	= [NUMERIC_PRECISION],
	[NumericScale]		= [NUMERIC_SCALE],
    [DatePrecision]		= [DATETIME_PRECISION],
    [IsIdentity]		= COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsIdentity'),
    [IsComputed]		= COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsComputed')
FROM  INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME <> '__RefactorLog'";

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
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }

        [ForeignKey(nameof(TableName))]
        public virtual Table Table { get; set; }

        public override string ToString()
        {
            return $"[{TableName}].[{ColumnName}]";
        }
    }
}
