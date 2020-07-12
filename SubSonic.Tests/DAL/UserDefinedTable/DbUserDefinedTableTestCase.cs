using SubSonic.Infrastructure.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SubSonic.Tests.DAL.UserDefinedTable
{
    using Extensions.Test;

    class DbUserDefinedTableTestCase<TEntity>
        : IDbUserDefinedTableTestCase
    {
        private readonly string expectation;
        private readonly string name;
        public DbUserDefinedTableTestCase(string expectation, string name = null)
        {
            this.expectation = expectation;
            this.name = name;
        }

        public string Expectation(string alias)
        {
            return expectation.Format(alias, Environment.NewLine);
        }

        public string Name => this.name;
        public IDbEntityModel Model => SubSonicContext.DbModel.GetEntityModel<TEntity>();
        public DataTable Data
        {
            get
            {
                return new[] { Activator.CreateInstance<TEntity>() }.ToDataTable();
            }
        }
    }
}
