using SubSonic.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.CodeGenerator.Models
{
    [DbView(Query = SQL)]
    public class Table
    {
        public const string SQL =
@"SELECT 
	[Schema]	= TABLE_SCHEMA,
	[Name]		= TABLE_NAME 
FROM  INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE' and TABLE_NAME <> '__RefactorLog'";

        public Table()
        {

        }

        public string Schema { get; set; }

        public string Name { get; set; }

        public virtual ISubSonicSetCollection<Relationship> Relationships { get; set; }
    }
}
