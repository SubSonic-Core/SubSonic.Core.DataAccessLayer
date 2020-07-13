using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace SubSonic.Extensions.Test.MockDbClient
{
    public class MockDbDataAdapter : DbDataAdapter
    {
        public MockDbDataAdapter(MockDbClientFactory provider)
            : base()
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            SelectCommand = provider.CreateCommand();
            UpdateCommand = provider.CreateCommand();
            InsertCommand = provider.CreateCommand();
            DeleteCommand = provider.CreateCommand();
        }

        public override int Fill(DataSet dataSet)
        {
            return base.Fill(dataSet);
        }
    }
}