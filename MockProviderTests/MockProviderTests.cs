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
        public void AddedTypeIsAvailable()
        {
            var m = new MockProvider();
            m.AddTransient<Foo>();
            Assert.IsNotNull(m.GetService<Foo>());
        }

        [TestMethod]
        public void NotAddedTypeIsNotAvailable()
        {
            var m = new MockProvider();
            m.AddTransient<Foo>();
            Assert.IsNull(m.GetService<Bus>());
        }

        [TestMethod]
        public void IServiceProviderIsAlwaysAvailableAndMockProviderItself()
        {
            var m = new MockProvider();
            var sp = m.GetService<IServiceProvider>();
            Assert.IsNotNull(sp);
            Assert.AreSame(m, sp);
        }

        [TestMethod]
        public void IServiceCollectionIsAlwaysAvailableAndMockProviderItself()
        {
            var m = new MockProvider();
            var sc = m.GetService<IServiceCollection>();
            Assert.IsNotNull(sc);
            Assert.AreSame(m, sc);
        }


        [TestMethod]
        public void GenericsAreCreatedCorrectly()
        {
            var m = new MockProvider();
            m.AddSingleton<Bar<Foo>>();

            var o = m.GetService<Bar<Foo>>();
            Assert.IsNotNull(o);
        }

        [TestMethod]
        public void GenericsAreCreatedCorrectlyWhenAddedAsGeneric()
        {
            var m = new MockProvider();
            m.AddSingleton<Foo>();
            m.AddTransient(typeof(Bar<>));

            var o = m.GetService<Bar<Foo>>();
            Assert.IsNotNull(o);
        }


        [TestMethod]
        public void GenericsAreCreatedCorrectlyWhenCreatedAsConstructorParameter()
        {
            var m = new MockProvider();
            m.AddSingleton<Foo>();
            m.AddTransient(typeof(Bar<>));
            m.AddSingleton<FooBar>();
            var o = m.GetService<FooBar>();
            Assert.IsNotNull(o);
        }

        [TestMethod]
        public void MocksCanBeAddedInContructorOfMockProvider()
        {
            var m1 = new Mock<Foo>();
            var m2 = new Mock<Bus>();
            var m = new MockProvider(m1, m2);

            var o = m.GetRequiredService<Bus>();
        }

        [TestMethod]
        public void MocksCanBeVerified_CalledIsVerified()
        {
            bool fooHasBeenCalled = false;
            var m1 = new Mock<Foo>();
            var m2 = new Mock<Bus>();
            m1.Setup(f => f.TestFoo()).Callback(() => fooHasBeenCalled = true).Verifiable();

            var m = new MockProvider(m1,m2);
            var foo = m.GetRequiredService<Foo>();

            foo.TestFoo();
            Assert.IsTrue(fooHasBeenCalled);
            m.Verify();
        }

        [TestMethod]
        public void MocksCanBeVerified_NotCalledNotVerified()
        {
            bool fooHasBeenCalled = false;
            var m1 = new Mock<Foo>();
            var m2 = new Mock<Bus>();
            m1.Setup(f => f.TestFoo()).Callback(() => fooHasBeenCalled = true).Verifiable();

            var m = new MockProvider(m1);
            var foo = m.GetRequiredService<Foo>();
            Assert.IsFalse(fooHasBeenCalled);
            try
            {
                m.Verify();
            }
            catch (MockException e)
            {
                Assert.IsTrue(e.IsVerificationError);
                Assert.IsTrue(e.Message.Contains("TestFoo")); 
            }
        }
    }
}
