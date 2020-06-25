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

            Logger = Context.Instance.GetService<ISubSonicLogger<DbTempTableBuilder>>();
        }

        const string expected_person = @"CREATE TABLE #Person(
	[ID] [Int] NOT NULL,
	[FirstName] [VarChar](50) NOT NULL,
	[MiddleInitial] [VarChar](1) NULL,
	[FamilyName] [VarChar](50) NOT NULL,
	[FullName] [VarChar](104) NULL,
	PRIMARY KEY CLUSTERED
		(
			[ID] ASC
		) WITH (IGNORE_DUP_KEY = OFF)
);";

        [Test]
        [TestCase(typeof(Models.Person), expected_person)]
        public void CanGenerateCreateTempTableForModel(Type modelType, string expected)
        {
            DbTempTableBuilder builder = new DbTempTableBuilder(
                Context.Model.GetEntityModel(modelType));

            string sql = null;

            using (var perf = Logger.Start("Generate SQL"))
            {
                sql = builder.GenerateSql();
            }

            sql.Should().Be(expected);

            Logger.LogInformation($"\n{sql}");
        }

        [Test]
        [TestCase(typeof(Models.Person))]
        public void CanGenerateDropTempTableForModel(Type modelType)
        {
            // TODO: Add your test code here
            DbTempTableBuilder builder = new DbTempTableBuilder(
                Context.Model.GetEntityModel(modelType));

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
                Context.Model.GetEntityModel(modelType));

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
