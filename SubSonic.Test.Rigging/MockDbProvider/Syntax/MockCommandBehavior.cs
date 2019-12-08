using System;
using System.Data;
using System.Data.Common;

namespace SubSonic.Test.Rigging.MockDbProvider.Syntax
{

    public class MockCommandBehavior
    {
        Predicate<DbCommand> match;
        internal object ReturnValue { get; private set; }

        internal bool Matches(DbCommand cmd)
        {
            return match(cmd);
        }

        public MockCommandBehavior When(Predicate<DbCommand> match)
        {
            this.match = match;
            return this;
        }

        public MockCommandBehavior ReturnsData(DataTable dt)
        {
            ReturnValue = dt;
            return this;
        }

        public MockCommandBehavior ReturnsScalar(object value)
        {
            ReturnValue = value;
            return this;
        }
    }
}