using FluentAssertions;
using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System.Data.Common;
using System.Linq;

namespace SubSonic.Tests.DAL.ExtensionMethod
{
    using Extensions.Test;
    using Extensions.Test.Data.Builders;
    using SubSonic.Extensions.Test.Models;

    [TestFixture]
    public partial class ExtensionMethodTests
        : BaseTestFixture
    {
        protected override void SetSelectBehaviors()
        {
            base.SetSelectBehaviors();

            string
                person_min = @"SELECT MIN([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                person_max = @"SELECT MAX([T1].[ID])
FROM [dbo].[Person] AS [T1]",
                renter_sum = @"SELECT SUM([T1].[Rent])
FROM [dbo].[Renter] AS [T1]
WHERE ([T1].[PersonID] = @personid_1)",
                renter_avg = @"SELECT AVG([T1].[Rent])
FROM [dbo].[Renter] AS [T1]
WHERE ([T1].[PersonID] = @personid_1)";
                

            string
                property_count_all = @"SELECT COUNT([T1].[ID]) [RECORDCOUNT]
FROM [dbo].[RealEstateProperty] AS [T1]",
                property_paged =
@"WITH page AS
(
	SELECT [T1].[ID]
	FROM [dbo].[RealEstateProperty] AS [T1]
	ORDER BY [T1].[ID]
	OFFSET @PageSize * (@PageNumber - 1) ROWS
	FETCH NEXT @PageSize ROWS ONLY
)
SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]
	INNER JOIN page
		ON ([page].[ID] = [T1].[ID])
OPTION (RECOMPILE)",
                renters_all = @"SELECT [T1].[PersonID], [T1].[UnitID], [T1].[Rent], [T1].[StartDate], [T1].[EndDate]
FROM [dbo].[Renter] AS [T1]";

            
            Context.Database.Instance.AddCommandBehavior(property_count_all, (cmd) =>
            {
                using (DataTableBuilder table = new DataTableBuilder())
                {
                    table
                    .AddColumn("RECORDCOUNT", typeof(int))
                    .AddRow(RealEstateProperties.Count());

                    return table.DataTable;
                }
            });
            Context.Database.Instance.AddCommandBehavior(property_paged, (cmd) =>
            {
                int size = 0, page = 0;

                foreach (DbParameter parameter in cmd.Parameters)
                {
                    if (parameter.ParameterName.Contains("PageSize"))
                    {
                        size = (int)parameter.Value;
                    }
                    else if (parameter.ParameterName.Contains("PageNumber"))
                    {
                        page = (int)parameter.Value;
                    }
                }

                return RealEstateProperties
                    .Skip(size * (page - 1))
                    .Take(size)
                    .ToDataTable();
            });


            Context.Database.Instance.AddCommandBehavior(person_max, cmd => People.Max(x => x.ID));
            Context.Database.Instance.AddCommandBehavior(person_min, cmd => People.Min(x => x.ID));

            Context.Database.Instance.AddCommandBehavior(renters_all, cmd => BuildDataTable(Renters));
            Context.Database.Instance.AddCommandBehavior(renter_sum, cmd => Renters.Where(x => x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>()).Sum(x => x.Rent));
            Context.Database.Instance.AddCommandBehavior(renter_avg, cmd => Renters.Where(x => x.PersonID == cmd.Parameters["@personid_1"].GetValue<int>()).Average(x => x.Rent));
        }

        [Test]
        public void TheCountMethodIsSupported()
        {
            Context.People.Count().Should().BeGreaterThan(0).And.IsOfType<int>();
        }

        [Test]
        public void TheLongCountMethodIsSupported()
        {
            Context.People.LongCount().Should().BeGreaterThan(0).And.IsOfType<long>();
        }

        [Test]
        public void TheMinMethodIsSupported()
        {
            Context.People.Min(x => x.ID).Should().Be(1);
        }

        [Test]
        public void TheMaxMethodIsSupported()
        {
            Context.People.Max(x => x.ID).Should().Be(People.Count);
        }

        [Test]
        public void TheSumMethodIsSupported()
        {
            Person person = Context.People.Single(x => x.ID == 1);

            person.Renters.Sum(x => x.Rent).Should().Be(Renters.Where(x => x.PersonID == person.ID).Sum(x => x.Rent));
        }

        [Test]
        public void TheAverageMethodIsSupported()
        {
            Renter renter = Context.Renters.First();

            renter.Person.Renters.Average(x => x.Rent).Should().Be(Renters.Where(x => x.PersonID == renter.PersonID).Average(x => x.Rent));
        }
    }
}
