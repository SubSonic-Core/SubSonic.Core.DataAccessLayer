using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SubSonic.Test.Rigging.Models
{
    public class Status
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
