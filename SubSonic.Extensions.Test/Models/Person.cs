using SubSonic.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.Extensions.Test.Models
{
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public virtual string FirstName { get; set; }

        [MaxLength(1)]
        public virtual string MiddleInitial { get; set; }

        [Required]
        [MaxLength(50)]
        public virtual string FamilyName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(104)]
        public string FullName { get; set; }

        public virtual ISubSonicCollection<Renter> Renters { get; set; }

        public virtual ISubSonicCollection<Unit> Units { get; set; }

        public override string ToString() => FullName;
    }
}
