using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyIoC.Tests.TestData;

namespace TinyIoC.Tests
{
    [TestClass]
    public class TinyIoCResolveFactoryDelegateTests
    {
        class Example
        {
            public delegate Example Factory(int value);

            public Example(int value, Dependency dependency)
            {
                Value = value;
                Dependency = dependency;
            }

            public int Value { get; private set; }
            public Dependency Dependency { get; private set; }
        }

        class Dependency
        {
        }

        [TestMethod]
        public void Resolve_FactoryDelegate_ReturnsDelegateThatCanConstructInstance()
        {
            var container = UtilityMethods.GetContainer();
            
            var factory = container.Resolve<Example.Factory>();
            var createdObject = factory(1);

            Assert.AreEqual(1, createdObject.Value);
            Assert.IsNotNull(createdObject.Dependency);
        }

        [TestMethod]
        public void Resolve_FactoryDelegate_CachesDelegate()
        {
            var container = UtilityMethods.GetContainer();

            var factory1 = container.Resolve<Example.Factory>();
            var factory2 = container.Resolve<Example.Factory>();

            Assert.AreSame(factory1, factory2);
        }
    }
}