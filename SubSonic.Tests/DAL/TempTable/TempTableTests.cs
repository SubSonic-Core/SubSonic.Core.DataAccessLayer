using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models = SubSonic.Extensions.Test.Models;
using Microsoft.Extensions.Logging;

namespace SubSonic.Tests.DAL.TempTable
{
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Logging;    

    [TestFixture]
    public class TempTableTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            Logger = DbContext.Instance.GetService<ISubSonicLogger<DbTempTableBuilder>>();
        }

        [Test]
        [TestCase(typeof(Models.Person))]
        public void CanGenerateCreateTempTableForModel(Type modelType)
        {
            // TODO: Add your test code here
            DbTempTableBuilder builder = new DbTempTableBuilder(
                DbContext.Model.GetEntityModel(modelType));

            string sql = null;

            using (var perf = Logger.Start("Generate SQL"))
            {
                sql = builder.GenerateSql();
            }

            sql.Should().Contain($"#{modelType.Name}");

            Logger.LogInformation($"\n{sql}");
        }

        [Test]
        [TestCase(typeof(Models.Person))]
        public void CanGenerateDropTempTableForModel(Type modelType)
        {
            // TODO: Add your test code here
            DbTempTableBuilder builder = new DbTempTableBuilder(
                DbContext.Model.GetEntityModel(modelType));

            string sql = null;

            using (var perf = Logger.Start("Generate Drop SQL"))
            {
                sql = builder.GenerateDropSql();
            }

            sql.Should().Contain($"#{modelType.Name}");

            Logger.LogInformation($"\n{sql}");
        }

        [Test]
        [TestCase(typeof(Models.Person))]
        public void CanGenerateSelectFromTempTableForModel(Type modelType)
        {
            // TODO: Add your test code here
            DbTempTableBuilder builder = new DbTempTableBuilder(
                DbContext.Model.GetEntityModel(modelType));

            string sql = null;

            using (var perf = Logger.Start("Generate Drop SQL"))
            {
                sql = builder.GenerateSelectSql();
            }

            sql.Should().Contain($"#{modelType.Name}");

            Logger.LogInformation($"\n{sql}");
        }
    }
}
