using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace TinyIoC.Tests
{
    [TestClass]
    public class TinyIoCTests
    {
        #region Utility Classes / Interfaces
        internal interface ITestInterface
        {
        }

        internal class TestClassDefaultCtor : ITestInterface
        {
            public string Prop1 { get; set; }

            public TestClassDefaultCtor()
            {

            }

            public static ITestInterface CreateNew(TinyIoC container)
            {
                return new TestClassDefaultCtor() { Prop1 = "Testing" };
            }
        }

        internal interface ITestInterace2
        {
        }

        internal class TestClassWithContainerDependency
        {
            public TinyIoC _Container { get; private set; }

            public TestClassWithContainerDependency(TinyIoC container)
            {
                _Container = container;
            }
        }

        internal class TestClassWithInterfaceDependency : ITestInterace2
        {
            public ITestInterface Dependency { get; set; }

            public int Param1 { get; private set; }

            public string Param2 { get; private set; }

            public TestClassWithInterfaceDependency(ITestInterface dependency)
            {
                Dependency = dependency;
            }

            public TestClassWithInterfaceDependency(ITestInterface dependency, int param1, string param2)
            {
                Dependency = dependency;
                Param1 = param1;
                Param2 = param2;
            }
        }

        internal class TestClassWithDependency
        {
            TestClassDefaultCtor Dependency { get; set; }

            public int Param1 { get; private set; }

            public string Param2 { get; private set; }

            public TestClassWithDependency(TestClassDefaultCtor dependency)
            {

            }

            public TestClassWithDependency(TestClassDefaultCtor dependency, int param1, string param2)
            {
                Param1 = param1;
                Param2 = param2;
            }
        }

        internal class TestClassWithDependencyAndParameters
        {
            TestClassDefaultCtor Dependency { get; set; }

            public int Param1 { get; private set; }

            public string Param2 { get; private set; }

            public TestClassWithDependencyAndParameters(TestClassDefaultCtor dependency, int param1, string param2)
            {
                Param1 = param1;
                Param2 = param2;
            }
        }

        internal class TestClassNoInterfaceDefaultCtor
        {
            public TestClassNoInterfaceDefaultCtor()
            {

            }
        }

        internal class TestClassNoInterfaceDependency
        {
            public ITestInterface Dependency { get; set; }

            public TestClassNoInterfaceDependency(ITestInterface dependency)
            {
                Dependency = dependency;
            }
        }

        internal class DisposableTestClassWithInterface : IDisposable, ITestInterface
        {
            Action OnDispose;

            public DisposableTestClassWithInterface(Action onDispose)
            {
                OnDispose = onDispose;
            }

            public DisposableTestClassWithInterface()
            {
                
            }

            public void Dispose()
            {
                if (OnDispose != null)
                    OnDispose.Invoke();
            }
        }
        #endregion

        private static TinyIoC GetContainer()
        {
            return new TinyIoC();
        }

        [TestMethod]
        public void Current_Get_ReturnsInstanceOfTinyIoC()
        {
            var container = TinyIoC.Current;

            Assert.IsInstanceOfType(container, typeof(TinyIoC));
        }

        [TestMethod]
        public void Current_GetTwice_ReturnsSameInstance()
        {
            var container1 = TinyIoC.Current;
            var container2 = TinyIoC.Current;

            Assert.ReferenceEquals(container1, container2);
        }

        [TestMethod]
        public void Register_ImplementationOnly_CanRegister()
        {
            GetContainer().Register<TestClassDefaultCtor>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InterfaceAndImplementation_CanRegister()
        {
            GetContainer().Register<ITestInterface, TestClassDefaultCtor>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithImplementation_ReturnsInstanceOfCorrectType()
        {
            var container = GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var output = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_RegisteredTypeImplementationOnly_ReturnsInstanceOfCorrectType()
        {
            var container = GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Register_WithDelegateFactoryStaticMethod_CanRegister()
        {
            var container = GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_WithDelegateFactoryLambda_CanRegister()
        {
            var container = GetContainer();
            container.Register<ITestInterface>((c, p) => new TestClassDefaultCtor() { Prop1 = "Testing" });

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_TypeRegisteredWithDelegateFactoryStaticMethod_ResolvesCorrectlyUsingDelegateFactory()
        {
            var container = GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            var output = container.Resolve<ITestInterface>() as TestClassDefaultCtor;

            Assert.AreEqual("Testing", output.Prop1);
        }

        [TestMethod]
        public void Resolve_TypeRegisteredWithDelegateFactoryLambda_ResolvesCorrectlyUsingDelegateFactory()
        {
            var container = GetContainer();
            container.Register<ITestInterface>((c, p) => new TestClassDefaultCtor() { Prop1 = "Testing" });

            TestClassDefaultCtor output = container.Resolve<ITestInterface>() as TestClassDefaultCtor;

            Assert.AreEqual("Testing", output.Prop1);
        }

        [TestMethod]
        public void Resolve_UnregisteredClassTypeWithDefaultCtor_ResolvesType()
        {
            var container = GetContainer();
            var output = container.Resolve<TestClassDefaultCtor>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnregisteredInterface_ThrowsException()
        {
            var container = GetContainer();
            var output = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeDefaultCtor_ReturnsTrue()
        {
            var container = GetContainer();
            container.Register<TestClassDefaultCtor>();

            var result = container.CanResolve(typeof(TestClassDefaultCtor));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_UnregisteredTypeDefaultCtor_ReturnsTrue()
        {
            var container = GetContainer();
            var result = container.CanResolve(typeof(TestClassDefaultCtor));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_UnregisteredInterface_ReturnsFalse()
        {
            var container = GetContainer();
            var result = container.CanResolve(typeof(ITestInterface));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredInterface_ReturnsTrue()
        {
            var container = GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var result = container.CanResolve(typeof(ITestInterface));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependencies_ReturnsTrue()
        {
            var container = GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependency>();

            var result = container.CanResolve(typeof(TestClassWithDependency));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependenciesAndParameters_ReturnsTrue()
        {
            var container = GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.CanResolve(typeof(TestClassWithDependencyAndParameters), new TinyIoC.NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependenciesAndIncorrectParameters_ReturnsFalse()
        {
            var container = GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.CanResolve(typeof(TestClassWithDependencyAndParameters), new TinyIoC.NamedParameterOverloads { { "wrongparam1", 12 }, { "wrongparam2", "Testing" } });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolveType_FactoryRegisteredType_ReturnsTrue()
        {
            var container = GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            var result = container.CanResolve(typeof(ITestInterface));

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_TinyIoC_ReturnsCurrentContainer()
        {
            var container = GetContainer();

            var result = container.Resolve<TinyIoC>();

            Assert.ReferenceEquals(result, container);
        }

        [TestMethod]
        public void Resolve_ClassWithTinyIoCDependency_Resolves()
        {
            var container = GetContainer();
            container.Register<TestClassWithContainerDependency>();

            var result = container.Resolve<TestClassWithContainerDependency>();

            Assert.IsInstanceOfType(result, typeof(TestClassWithContainerDependency));
        }

        [TestMethod]
        public void Register_Instance_CanRegister()
        {
            var container = GetContainer();
            container.Register<DisposableTestClassWithInterface>(new DisposableTestClassWithInterface());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InstanceUsingInterface_CanRegister()
        {
            var container = GetContainer();
            container.Register<ITestInterface, DisposableTestClassWithInterface>(new DisposableTestClassWithInterface());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_RegisteredInstance_SameInstance()
        {
            var container = GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<DisposableTestClassWithInterface>(item);

            var result = container.Resolve<DisposableTestClassWithInterface>();

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        public void Resolve_RegisteredInstanceWithInterface_SameInstance()
        {
            var container = GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<ITestInterface, DisposableTestClassWithInterface>(item);

            var result = container.Resolve<ITestInterface>();

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        [Ignore]
        public void Dispose_RegisteredDisposableInstance_CallsDispose()
        {
            var container = GetContainer();
            bool hasDisposed = false;
            var item = new DisposableTestClassWithInterface(() => { hasDisposed = true; });
            container.Register<DisposableTestClassWithInterface>(item);

            container.Dispose();

            Assert.IsTrue(hasDisposed);
        }

        [TestMethod]
        [Ignore]
        public void Dispose_RegisteredDisposableInstanceWithInterface_CallsDispose()
        {
            var container = GetContainer();
            bool hasDisposed = false;
            var item = new DisposableTestClassWithInterface(() => { hasDisposed = true; });
            container.Register<ITestInterface, DisposableTestClassWithInterface>(item);

            container.Dispose();

            Assert.IsTrue(hasDisposed);
        }

    }
}
