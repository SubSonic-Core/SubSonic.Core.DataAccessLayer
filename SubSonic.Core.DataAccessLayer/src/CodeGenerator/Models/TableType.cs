using SubSonic.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.CodeGenerator.Models
{
    [DbView(Query = SqlQuery)]
    public class TableType
    {
        public const string SqlQuery = @"select 
	[Id] = CAST(tt.type_table_object_id as bigint),
	[SchemaOwner] = s.[name],
	[Name] = tt.[name]
from sys.table_types tt
	join sys.schemas s on s.schema_id = tt.schema_id
where tt.is_table_type = 1
	";

        public long Id { get; set; }
        public string SchemaOwner { get; set; }
        public string Name { get; set; }
    }
}
