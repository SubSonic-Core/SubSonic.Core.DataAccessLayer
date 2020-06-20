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
        public virtual string FirstName { get; set; }

        public virtual string MiddleInitial { get; set; }

        [Required]
        public virtual string FamilyName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }

        public virtual ICollection<Renter> Renters { get; set; }
    }
}
