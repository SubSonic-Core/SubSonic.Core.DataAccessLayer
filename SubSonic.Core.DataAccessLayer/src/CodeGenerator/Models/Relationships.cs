using SubSonic.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.CodeGenerator.Models
{
    [DbView(Query = Query)]
    public class Relationship
    {
        public const string Query =
@"SELECT
    TableName  = FK.TABLE_NAME,
    ColumnName = CU.COLUMN_NAME,
    ForiegnTableName  = PK.TABLE_NAME,
    ForiegnColumnName = PT.COLUMN_NAME, 
    ConstraintName = C.CONSTRAINT_NAME,
    SchemaOwner = FK.TABLE_SCHEMA
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
INNER JOIN
    (	
        SELECT i1.TABLE_NAME, i2.COLUMN_NAME
        FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
        WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
    ) 
PT ON PT.TABLE_NAME = PK.TABLE_NAME";

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public string ForiegnTableName { get; set; }

        public string ForiegnColumnName { get; set; }
        [Key]
        public string ConstraintName { get; set; }

        public string SchemaOwner { get; set; }

        [ForeignKey(nameof(TableName))]
        public virtual Table Table { get; set; }

        [ForeignKey(nameof(ForiegnTableName))]
        public virtual Table ForiegnTable { get; set; }

        public override string ToString()
        {
            return ConstraintName;
        }
    }
}
