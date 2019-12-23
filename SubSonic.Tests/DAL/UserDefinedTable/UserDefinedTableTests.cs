using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SubSonic.Infrastructure.Logging;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.UserDefinedTable
{
    using FluentAssertions;
    using Infrastructure;
    using System.Data;

    [TestFixture]
    public class UserDefinedTableTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            logger = DbContext.Instance.GetService<ISubSonicLogger<DbUserDefinedTableBuilder>>();
        }
        [Test]
        [TestCase(typeof(Models.RealEstateProperty))]
        [TestCase(typeof(Models.Unit))]
        [TestCase(typeof(Models.Renter))]
        public void CanGenerateUserDefinedTableForModel(Type modelType)
        {
            IEnumerable data = null;

            if(modelType == typeof(Models.RealEstateProperty))
            {
                data = RealEstateProperties;
            }
            else if (modelType == typeof(Models.Unit))
            {
                data = Units;
            }
            else if (modelType == typeof(Models.Renter))
            {
                data = Renters;
            }


            DbUserDefinedTableBuilder builder = new DbUserDefinedTableBuilder(
                DbContext.Model.GetEntityModel(modelType),
                data);

            string sql = null;

            using (var perf = logger.Start("Generate SQL"))
            {
                 sql = builder.GenerateSql();
            }

            DataTable table = null;

            using (var perf = logger.Start("Generate Data"))
            {
                table = builder.GenerateTable();
            }

            table.Dispose();

            logger.LogInformation($"\n{sql}");
        }
    }
}
