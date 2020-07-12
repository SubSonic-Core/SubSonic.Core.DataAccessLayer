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
    using System.Reflection;
    using SubSonic.Infrastructure;

    [TestFixture]
    public class DbWherePredicateBuilderTests
        : SUT.BaseTestFixture
    {
        interface IPredicateTestCase
        {
            Expression Predicate { get; }
            Type Type { get; }
            Type DbSetType { get; }
            Expression Expression { get; }
        }
        class GetPredicateFor<TEntity>
            : IPredicateTestCase
            where TEntity : class
        {
            public GetPredicateFor(Expression<Func<TEntity, bool>> selector)
            {
                Predicate = selector;
            }

            public Type Type => typeof(TEntity);

            public Type DbSetType => typeof(ISubSonicSetCollection<>).MakeGenericType(Type);

            public Expression Predicate { get; }

            public Expression Expression
            {
                get
                {
                    MethodInfo method = typeof(Queryable).GetGenericMethod(nameof(Queryable.Where), new[] { DbSetType, Predicate.GetType() });

                    return DbExpression.DbWhere(method, new[] { 
                        SubSonicContext.Current.Set<TEntity>()?.Expression, 
                        Predicate });
                }
            }
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
            foreach(IPredicateTestCase @case in Expressions())
            //Parallel.ForEach(Expressions(), @case =>
            {
                var result = DbWherePredicateBuilder.GetWhereTranslation(@case.Expression);

                if (result is DbWhereExpression where)
                {
                    where.Parameters.Count.Should().NotBe(0);

                    where.Type.Should().Be(typeof(bool));

                    where.GetArgument(0).Type.GetInterface(@case.DbSetType.Name).Should().NotBeNull();

                    where.Expression.Should().NotBeNull();
                }

                result.Should().NotBeNull();
            }
            //);
        }
    }
}
