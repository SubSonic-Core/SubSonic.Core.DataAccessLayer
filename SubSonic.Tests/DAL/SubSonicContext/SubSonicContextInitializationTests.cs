using FluentAssertions;
using NUnit.Framework;
using SubSonic.src;
using SubSonic.Tests.DAL.SUT;
using System;

namespace SubSonic.Tests.DAL
{
    [TestFixture]
    [Order(-100)]
    public class SubSonicContextInitializationTests
    {
        [Test]
        public void SubSonicContextShouldFailIfServiceProviderIsNotSet()
        {
            FluentActions.Invoking(() => new BadSubSonicContext()).Should().Throw<InvalidOperationException>().WithMessage(SubSonicErrorMessages.ConfigurationInvalid.Format(nameof(DbContextOptionsBuilder.SetServiceProvider)));
        }
    }
}
