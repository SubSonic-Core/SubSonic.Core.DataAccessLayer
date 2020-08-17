using SubSonic.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SubSonic.CodeGenerator.Models
{
    [DbView(Query = Query)]
    public class Table
    {
        public const string Query =
@"SELECT 
    [Catalog]	= TABLE_CATALOG,
	[Schema]	= TABLE_SCHEMA,
	[Name]		= TABLE_NAME 
FROM  INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'";

        public Table()
        {

        }
        
        public string Catalog { get; set; }
        public string Schema { get; set; }
        [Key]
        public string Name { get; set; }

        public virtual ISubSonicCollection<Relationship> WithOneRelationships { get; set; }

        public virtual ISubSonicCollection<Relationship> WithManyRelationships { get; set; }

        public virtual ISubSonicCollection<Column> Columns { get; set; }

        public override string ToString()
        {
            return $"[{Schema}].[{Name}]";
        }
    }
}
