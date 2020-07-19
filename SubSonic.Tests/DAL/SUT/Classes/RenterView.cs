using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL
{
    public class RenterView
    {
        public int PersonID { get; set; }
        public string FullName { get; set; }
        public decimal Rent { get; set; }
        public string Status { get; internal set; }
        public int UnitID { get; internal set; }
        public bool HasParallelPowerGeneration { get; internal set; }
    }
}
