using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.Extensions.Test.Models
{
    [Table(nameof(Unit))]
    public class Unit
    {
        [Key]
        public int ID { get; set; }

        public int RealEstatePropertyID { get; set; }

        [ForeignKey(nameof(RealEstatePropertyID))]
        public virtual RealEstateProperty RealEstateProperty { get;set;}

        public int StatusID { get; set; }

        [ForeignKey(nameof(StatusID))]
        public virtual Status Status { get; set; }

        [Column("Bedrooms", TypeName = "int")]
        public int NumberOfBedrooms { get; set; }
    }
}
