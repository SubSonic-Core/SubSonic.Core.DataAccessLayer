using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public abstract class DbSubSonicStoredProcedure
    {
        protected DbSubSonicStoredProcedure()
        {

        }

        public abstract int Result { get; set; }
    }
}
