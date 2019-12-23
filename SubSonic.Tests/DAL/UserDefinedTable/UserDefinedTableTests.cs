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
    using Infrastructure;

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
        public void CanGenerateUserDefinedTableForModelRealEstateProperty()
        {
            DbUserDefinedTableBuilder builder = new DbUserDefinedTableBuilder(
                DbContext.Model.GetEntityModel<Models.RealEstateProperty>(),
                RealEstateProperties);

            string sql = null;

            using (var perf = logger.Start("Generate SQL"))
            {
                 sql = builder.GenerateSql();
            }

            logger.LogInformation($"\n{sql}");
        }
    }
}
