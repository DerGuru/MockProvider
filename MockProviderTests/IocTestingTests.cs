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
            var asm = typeof(Tests).Assembly;
            var missings = asm.FindMissingRegistrations<IMarker>(IocTest.ConfigureGoodCase<Foo>);
            Assert.IsFalse(missings.Any());
        }

        [TestMethod]
        [Ignore] //this test is supposed to fail, to show how to put the data 
        public void NotAddedTypeIsNotAvailable()
        {
            var asm = typeof(Tests).Assembly;
            var missings = asm.FindMissingRegistrations<IMarker>(IocTest.ConfigureBadCase);
            Assert.IsFalse(missings.Any(), missings.First().ToString());
        }

    }

    public class IocTest : IMarker
    {
        public IocTest(Foo foo) { }
        public IocTest(Bar bar) { }

        public static void ConfigureGoodCase<T>(IServiceCollection s) where T : class
        {
            s.AddTransient<T>();
        }

        public static void ConfigureBadCase(IServiceCollection s)
        {

        }
    }

    public interface IMarker
    {
    }
}
