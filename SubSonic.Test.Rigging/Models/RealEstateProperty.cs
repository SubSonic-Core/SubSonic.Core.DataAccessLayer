using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.Test.Rigging.Models
{
    public class RealEstateProperty
    {
        public RealEstateProperty()
        {
            Units = new HashSet<Unit>();
        }

        [Key]
        public int ID { get; set; }

        public int StatusID { get; set; }

        [ForeignKey(nameof(StatusID))]
        public virtual Status Status { get; set; }

        public virtual ICollection<Unit> Units { get; set; }
    }
}
