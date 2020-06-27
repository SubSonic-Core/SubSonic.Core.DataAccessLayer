using NUnit.Framework;
using SubSonic.Extensions.Test.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace SubSonic.Tests.DAL.Builders
{
    using Linq.Expressions;
    using Infrastructure.Builders;
    using Models = Extensions.Test.Models;
   
    [TestFixture]
    public class DbWherePredicateBuilderTests
        : SUT.BaseTestFixture
    {
        interface IPredicateTestCase
        {
            LambdaExpression Predicate { get; }
            Type Type { get; }
            DbTableExpression Table { get; }
        }
        class GetPredicateFor<TEntity>
            : IPredicateTestCase
        {
            public GetPredicateFor(Expression<Func<TEntity, bool>> selector)
            {
                Predicate = selector;
            }

            public Type Type => typeof(TEntity);

            public LambdaExpression Predicate { get; }

            public DbTableExpression Table => DbContext.DbModel.GetEntityModel<TEntity>().Table;
        }

        private static IEnumerable<IPredicateTestCase> Expressions()
        {
            yield return new GetPredicateFor<Models.Renter>(x => x.PersonID == 1 && x.UnitID == 1);
            yield return new GetPredicateFor<Models.Person>(x => x.ID == 1);
            yield return new GetPredicateFor<Models.Unit>(x => x.ID == 1);
        }
        
        [Test]
        public void ShouldBeAbleToGetWherePredicateBody()
        {
            //foreach(IPredicateTestCase @case in Expressions())
            Parallel.ForEach(Expressions(), @case =>
            {
                var result = DbWherePredicateBuilder.GetWherePredicate(@case.Type, @case.Predicate, DbExpressionType.Where, @case.Table);

                result.Should().NotBeNull();
            }
            );
        }
    }
}
