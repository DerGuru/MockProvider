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
            var m = new MockProvider(m1, m2);
            m1.Setup(f => f.TestFoo()).Callback(() => fooHasBeenCalled = true).Verifiable();

            var foo = m.GetRequiredService<Foo>();

            foo.TestFoo();
            Assert.IsTrue(fooHasBeenCalled);
            m.Verify();
        }

        [TestMethod]
        public void MocksCanBeVerified_NotCalledNotVerified()
        {

            var m1 = new Mock<Foo>();
            var m2 = new Mock<Bus>();
            m1.Setup(f => f.TestFoo()).Verifiable();

            var m = new MockProvider(m1);
            m.Add(m2);
            var foo = m.GetRequiredService<Foo>();

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

        [TestMethod]
        public void CreatedMocksAreAdded()
        {
            var m = new MockProvider();
            m.CreateMock<Foo>();

            Assert.IsNotNull(m.GetService<Foo>());
        }

        [TestMethod]
        public void ProForma()
        {
            var m = new MockProvider();
            Assert.AreEqual(2, m.Count);

            m.CreateMock<Foo>();
            Assert.AreEqual(3, m.Count);

            var md = m[2] as MockDescriptor;
            Assert.AreEqual(2, m.IndexOf(md));

            var foo = m.GetService<Foo>();
            Assert.AreSame(md.Instance, foo);

            Assert.IsTrue(m.Contains(md));

            Assert.IsTrue(m.Remove(md));
            Assert.AreEqual(2, m.Count);
            Assert.IsFalse(m.Contains(md));

            var id = new InstanceDescriptor(typeof(Bar<Foo>),new Bar<Foo>(m));
            m.Insert(2, id);

            m.RemoveAt(2);
            Assert.AreEqual(2, m.Count);
            m[1] = id;
            Assert.AreSame(m[1], id);

            m.Clear();
            Assert.AreEqual(2, m.Count);

            m.CreateMock<Bar<Foo>>(m);
            var target = new ServiceDescriptor[m.Count];
            m.CopyTo(target, 0);

            foreach (var mde in m)
            {
                int i = m.IndexOf(mde);
                Assert.AreSame(mde, target[i]);
            }

            Assert.IsFalse(m.IsReadOnly);

            m.Clear();

            m.AddTransient(typeof(Bar<>));

            m.CreateMock<Bar<Bus>>(m);
        }
    }
}
