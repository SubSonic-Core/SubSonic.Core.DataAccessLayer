using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SubSonic.Extensions.Test.Models
{
    public class Status
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
