using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using System.Data;
using System.Reflection;

namespace SubSonic.Tests.DAL.UserDefinedTable
{
    using Tests.DAL.SUT;
    using Infrastructure.Logging;
    using Infrastructure;
    using Data.Caching;
    using Extensions.Test;
    using Infrastructure.Schema;
    using Linq.Expressions;
    using Models = Extensions.Test.Models;

    [TestFixture]
    public partial class UserDefinedTableTests
    {
        private static IEnumerable<IDbUserDefinedTableTestCase> UserDefinedTableTestCases()
        {
            yield return new DbUserDefinedTableTestCase<Models.Person>("SELECT [{0}].[ID], [{0}].[FirstName], [{0}].[MiddleInitial], [{0}].[FamilyName], [{0}].[FullName]{1}FROM @person AS [{0}]");
            yield return new DbUserDefinedTableTestCase<Models.RealEstateProperty>("SELECT [{0}].[ID], [{0}].[StatusID], [{0}].[HasParallelPowerGeneration]{1}FROM @property AS [{0}]", "property");
            yield return new DbUserDefinedTableTestCase<Models.Renter>("SELECT [{0}].[PersonID], [{0}].[UnitID], [{0}].[Rent], [{0}].[StartDate], [{0}].[EndDate]{1}FROM @renter AS [{0}]");
        }

        [Test]
        [TestCaseSource(nameof(UserDefinedTableTestCases))]
        public void CanGenerateSelectFromUserDefinedTableType(IDbUserDefinedTableTestCase dbTest)
        {
            DbExpression select = null;

            if (!dbTest.Model.DefinedTableTypeExists)
            {
                using (dbTest.Model.AlteredState<IDbEntityModel, DbEntityModel>(new
                {
                    DefinedTableType = new DbUserDefinedTableTypeAttribute(dbTest.Model.Name)
                }).Apply())
                {
                    select = DbExpression.DbSelect(dbTest.Data, dbTest.Data.GetType(), dbTest.Model.GetTableType(dbTest.Name));
                }
            }
            else
            {
                select = DbExpression.DbSelect(dbTest.Data, dbTest.Data.GetType(), dbTest.Model.GetTableType(dbTest.Name));
            }
            
            select.ToString().Should().Be(dbTest.Expectation("T1"));
        }

        [Test]
        [TestCaseSource(nameof(UserDefinedTableTestCases))]
        public void ShouldThrowWhenGetTableTypePassedEmptyString(IDbUserDefinedTableTestCase dbTest)
        {
            FluentActions.Invoking(() =>
            {
                DbExpression.DbSelect(dbTest.Data, dbTest.Data.GetType(), dbTest.Model.GetTableType(string.Empty));
            }).Should().Throw<InvalidOperationException>();
        }
    }
}
