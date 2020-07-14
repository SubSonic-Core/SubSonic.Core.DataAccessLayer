using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace SubSonic.Tests.DAL.ExtensionMethod
{
    using Linq;
    using SubSonic.Extensions.Test.Models;
    using System.Threading.Tasks;
    using Models = Extensions.Test.Models;


    public partial class ExtensionMethodTests
    {
        [Test]
        public void ShouldBeAbleToPageData()
        {
            int
                recordCount = RealEstateProperties.Count(),
                pageSize = 3,
                pageCount = (int)Math.Ceiling((decimal)recordCount / pageSize);            

            IDbPageCollection<Models.RealEstateProperty> collection = Context.RealEstateProperties.ToPagedCollection(pageSize);

            collection.PageSize.Should().Be(pageSize);

            collection.GetRecordsForPage(1).Count().Should()
                .BeGreaterOrEqualTo(pageSize)
                .And
                .BeLessThan(collection.RecordCount);

            collection.PageNumber.Should().Be(1);

            collection.GetRecordsForPage(2).Count().Should().BeLessOrEqualTo(pageSize);
            collection.PageNumber.Should().Be(2);

            collection.RecordCount.Should().Be(recordCount);
            collection.PageCount.Should().Be(pageCount);
        }

        [Test]
        public void ShouldBeAbleToEnumeratePagedData()
        {
            int
                recordCount = RealEstateProperties.Count(),
                pageSize = 3,
                pageCount = (int)Math.Ceiling((decimal)recordCount / pageSize),
                cnt = 0;

            IDbPageCollection<Models.RealEstateProperty> collection = Context.RealEstateProperties.ToPagedCollection(pageSize);

            bool enumerated = false;

            foreach (IDbPageCollection<Models.RealEstateProperty> page in collection.GetPages())
            {
                enumerated |= true;

                cnt++;

                page.RecordCount.Should().Be(recordCount);

                if (page.PageNumber == 1)
                {
                    page.Count().Should()
                        .Be(pageSize)
                        .And
                        .BeLessThan(page.RecordCount);
                }
                else if (page.PageNumber == 2)
                {
                    page.Count().Should()
                        .BeLessOrEqualTo(page.PageSize)
                        .And
                        .BeLessThan(page.RecordCount);
                }
            }

            enumerated.Should().BeTrue();
            collection.PageCount.Should().Be(pageCount).And.Be(cnt);
        }

        [Test]
        public async Task ShouldBeAbleToEnumeratePagedDataAsync()
        {
            int
                recordCount = People.Count(),
                pageSize = 10,
                pageCount = (int)Math.Ceiling((decimal)recordCount / pageSize),
                cnt = 0, 
                pageCnt = 0;

            foreach (var page in Context.People.ToPagedCollection(pageSize).GetPages())
            {
                pageCnt++;

                await foreach (Person person in page)
                {
                    cnt++;
                }
            }

            pageCnt.Should().Be(pageCount);
            cnt.Should().Be(recordCount);
        }
    }
}
