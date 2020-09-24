using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MockProviderTests
{
    [TestClass]
    public class MockTests
    {
        [TestMethod]
        public void Test1()
        {
            var m = new MockProvider();
            m.AddTransient<Foo>();
            m.AddTransient(typeof(Bar<>));
            var o = m.GetRequiredService<Foo>();
        }

        [TestMethod]
        public void Test2()
        {
            var m1 = new Mock<Foo>(null);
            var m2 = new Mock<IServiceScope>();
            var m = new MockProvider(m1, m2);

            var o = m.GetRequiredService<IServiceScope>();
        }


    }
    public class Foo
    {
        public Foo(Bar<Foo> foo) { }
    }

    public class Bar<T>
    {

    }
}
