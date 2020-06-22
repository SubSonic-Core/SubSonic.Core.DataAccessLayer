using System;
using System.Data;
using System.Data.Common;

namespace SubSonic.Extensions.Test.MockDbClient.Syntax
{

    public class MockCommandBehavior
    {
        Predicate<DbCommand> match;

        private int recieved;

        internal object ReturnValue { get; private set; }

        internal bool Matches(DbCommand cmd)
        {
            bool result = match(cmd);

            if (result)
            {
                ++recieved;
            }

            return result;
        }

        public int GetRecievedCount()
        {
            --recieved;

            return recieved;
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