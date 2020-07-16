using Bogus;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;

namespace SubSonic.Tests.DAL
{
    [SetUpFixture]
    public class SetUpSubSonic
    {
        public static TestSubSonicContext DbContext { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DbContext = new TestSubSonicContext();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DbContext.Dispose();
            DbContext = null;
        }
    }
}