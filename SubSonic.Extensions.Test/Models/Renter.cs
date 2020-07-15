using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubSonic.Extensions.Test.Models
{
    public class Renter
    {
        [Key]
        public int ID { get; set; }

        [Key]
        public int PersonID { get; set; }
        [Key]
        public int UnitID { get; set; }

        [ForeignKey(nameof(PersonID))]
        public virtual Person Person { get; set; }

        [ForeignKey(nameof(UnitID))]
        public virtual Unit Unit { get; set; }

        public virtual decimal Rent { get; set; }

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

    }
}
