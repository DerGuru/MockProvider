using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockProviderTests;
using System.Linq;

namespace Ioc
{
    [TestClass]
    public class Tests
    {

        [TestMethod]
        public void AddedTypeIsAvailable()
        {
            var missings = IocTesting.FindMissingRegistrations<IMarker>(IocTest.ConfigureGoodCase);
            Assert.IsFalse(missings.Any());
        }

        [TestMethod]
        [Ignore] //this test is supposed to fail, to show how to put the data 
        public void NotAddedTypeIsNotAvailable()
        {
            var missings = IocTesting.FindMissingRegistrations<IMarker>(IocTest.ConfigureBadCase);
            Assert.IsFalse(missings.Any(), missings.First().ToString());
        }

    }

    public class IocTest : IMarker
    {
        public IocTest(Foo foo) { }

        public static void ConfigureGoodCase(IServiceCollection s)
        {
            s.AddTransient<Foo>();
        }

        public static void ConfigureBadCase(IServiceCollection s)
        {

        }
    }

    public interface IMarker
    {
    }
}
