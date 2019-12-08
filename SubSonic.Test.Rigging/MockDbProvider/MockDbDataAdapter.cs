using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace SubSonic.Test.Rigging.MockDbProvider
{
    public class MockDbDataAdapter : DbDataAdapter
    {
        public MockDbDataAdapter(MockDbProviderFactory provider)
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
    }
}