using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace SubSonic.Tests.DAL.ExtensionMethod
{
    using Linq;
    using Microsoft.Extensions.Logging;
    using SubSonic.Extensions.Test;
    using SubSonic.Extensions.Test.Models;
    using SubSonic.Logging;
    using System.Threading.Tasks;
    using Models = Extensions.Test.Models;


    public partial class ExtensionMethodTests
    {
        [Test]
        public void CanGetMemberInitializedUsingDatasource()
        {
            var logging = Context.Instance.GetService<ISubSonicLogger<RenterView>>();

            using (var perf = logging.Start(nameof(CanGetMemberInitializedUsingDatasource)))
            {
                var query = Context.Renters
                    .Where(x =>
                        DateTime.Today.Between(x.StartDate, x.EndDate.GetValueOrDefault(DateTime.Today.AddDays(1))))
                    .Include(x => x.Unit)
                    .ThenInclude(x => x.RealEstateProperty)
                    .ThenInclude(x => x.Status)
                    .Include(x => x.Person)
                    .Select(x => new RenterView()
                    {
                        PersonID = x.PersonID,
                        FullName = x.Person.FullName,
                        Rent = x.Rent,
                        UnitID = x.UnitID,
                        HasParallelPowerGeneration = x.Unit.RealEstateProperty.HasParallelPowerGeneration.GetValueOrDefault(),
                        Status = x.Unit.Status.Name
                    });

                logging.LogInformation(query.Expression.ToString());

                query.Count().Should().BeGreaterThan(0);

                bool iterated = false;

                foreach (RenterView view in query)
                {
                    iterated = true;

                    view.FullName.Should().Be(People.Single(x => x.ID == view.PersonID).FullName);
                }

                Context.Database.Instance.RecievedCommand(@"SELECT [T1].[PersonID], [T2].[FullName], [T1].[Rent], [T1].[UnitID], COALESCE([T3].[HasParallelPowerGeneration], 0) AS [HasParallelPowerGeneration], [T4].[Name] AS [Status]
FROM [dbo].[Renter] AS [T1]
	INNER JOIN [dbo].[Unit] AS [T5]
		ON ([T5].[ID] = [T1].[UnitID])
	INNER JOIN [dbo].[RealEstateProperty] AS [T3]
		ON ([T3].[ID] = [T5].[RealEstatePropertyID])
	INNER JOIN [dbo].[Status] AS [T4]
		ON ([T4].[ID] = [T3].[StatusID])
	INNER JOIN [dbo].[Person] AS [T2]
		ON ([T2].[ID] = [T1].[PersonID])
WHERE @dt_value_1 BETWEEN [T1].[StartDate] AND COALESCE([T1].[EndDate], @dt_value_2)");

                iterated.Should().BeTrue();
            }
        }
    }
}
