using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.Extensions.Test.Models
{
    public class Occupant
    {
        [Key]
        public int ID { get; set; }

        public int PersonID { get; set; }

        public int UnitID { get; set; }

        [ForeignKey(nameof(UnitID))]
        public virtual Unit Unit { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

    }
}
