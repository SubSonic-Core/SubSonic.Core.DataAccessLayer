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
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// setting up a output parameter for errors is more efficent instead of using a try catch.
        /// </remarks>
        public virtual string Error { get; set; }

        public abstract int Result { get; set; }
    }
}
