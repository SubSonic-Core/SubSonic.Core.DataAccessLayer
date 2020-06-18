using SubSonic.Infrastructure.Schema;
using System.Collections.Generic;
using System.Data;

namespace SubSonic.Tests.DAL.UserDefinedTable
{
    public interface IDbUserDefinedTableTestCase
    {
        string Expectation(string alias);
        string Name { get; }
        IDbEntityModel Model { get; }
        DataTable Data { get; }
    }
}