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
    }
}
