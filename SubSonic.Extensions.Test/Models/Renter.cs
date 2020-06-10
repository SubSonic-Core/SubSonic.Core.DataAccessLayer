using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubSonic.Extensions.Test.Models
{
    using Infrastructure;

    [DbUserDefinedTableType(nameof(Renter))]
    public class Renter
    {
        [Key]
        public int PersonID { get; set; }
        [Key]
        public int UnitID { get; set; }

        [ForeignKey(nameof(PersonID))]
        public virtual Person Person { get; set; }

        [ForeignKey(nameof(UnitID))]
        public virtual Unit Unit { get; set; }

        public decimal Rent { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

    }
}
