using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

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
            m.Verify();
        }

        [TestMethod]
        public void Test2()
        {
            var m1 = new Mock<Foo>(null);
            var m2 = new Mock<IServiceScope>();
            var m = new MockProvider(m1, m2);

            var o = m.GetRequiredService<IServiceScope>();
            m.Verify();
        }


    }
    public class Foo
    {
        public Foo(Bar<Foo> foo) { }
    }

    public class Bar<T>
    {
        public Bar(IServiceProvider sp)
        { 
        }
    }
}
