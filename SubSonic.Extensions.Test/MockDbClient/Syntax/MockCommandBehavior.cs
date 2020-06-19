using System;
using System.Data;
using System.Data.Common;

namespace SubSonic.Extensions.Test.MockDbClient.Syntax
{

    public class MockCommandBehavior
    {
        Predicate<DbCommand> match;

        public int Recieved { get; private set; }

        internal object ReturnValue { get; private set; }

        internal bool Matches(DbCommand cmd)
        {
            bool result = match(cmd);

            if (result)
            {
                ++Recieved;
            }

            return result;
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

        public MockCommandBehavior ReturnsData<TResult>(Func<DbCommand, TResult> command)
        {
            ReturnValue = command;

            return this;
        }

        public MockCommandBehavior ReturnsScalar(object value)
        {
            ReturnValue = value;
            return this;
        }
    }
}