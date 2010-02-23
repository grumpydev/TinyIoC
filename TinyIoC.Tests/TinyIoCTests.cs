using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using TinyIoC.Tests.TestData;
using TinyIoC.Tests.TestData.BasicClasses;

namespace TinyIoC.Tests
{
    [TestClass]
    public class TinyIoCTests
    {
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
            UtilityMethods.GetContainer().Register<TestClassDefaultCtor>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InterfaceAndImplementation_CanRegister()
        {
            UtilityMethods.GetContainer().Register<ITestInterface, TestClassDefaultCtor>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithImplementation_ReturnsInstanceOfCorrectType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var output = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithImplementation_ReturnsSingleton()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var output = container.Resolve<ITestInterface>();
            var output2 = container.Resolve<ITestInterface>();

            Assert.ReferenceEquals(output, output2);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeImplementationOnly_ReturnsInstanceOfCorrectType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_RegisteredTypeImplementationOnly_ReturnsMultipleInstances()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>();
            var output2 = container.Resolve<TestClassDefaultCtor>();

            Assert.IsFalse(object.ReferenceEquals(output, output2));
        }

        [TestMethod]
        public void Register_WithDelegateFactoryStaticMethod_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_WithDelegateFactoryLambda_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => new TestClassDefaultCtor() { Prop1 = "Testing" });

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_TypeRegisteredWithDelegateFactoryStaticMethod_ResolvesCorrectlyUsingDelegateFactory()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            var output = container.Resolve<ITestInterface>() as TestClassDefaultCtor;

            Assert.AreEqual("Testing", output.Prop1);
        }

        [TestMethod]
        public void Resolve_TypeRegisteredWithDelegateFactoryLambda_ResolvesCorrectlyUsingDelegateFactory()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => new TestClassDefaultCtor() { Prop1 = "Testing" });

            TestClassDefaultCtor output = container.Resolve<ITestInterface>() as TestClassDefaultCtor;

            Assert.AreEqual("Testing", output.Prop1);
        }

        [TestMethod]
        public void Resolve_UnregisteredClassTypeWithDefaultCtor_ResolvesType()
        {
            var container = UtilityMethods.GetContainer();
            var output = container.Resolve<TestClassDefaultCtor>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_UnregisteredClassTypeWithDependencies_ResolvesType()
        {
            var container = UtilityMethods.GetContainer();

            var output = container.Resolve<TestClassWithDependency>();

            Assert.IsInstanceOfType(output, typeof(TestClassWithDependency));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnregisteredInterface_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            var output = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnregisteredClassWithUnregisteredInterfaceDependencies_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            var output = container.Resolve<TestClassWithInterfaceDependency>();

            Assert.IsInstanceOfType(output, typeof(TestClassWithInterfaceDependency));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredClassWithUnregisteredInterfaceDependencies_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithInterfaceDependency>();

            var output = container.Resolve<TestClassWithInterfaceDependency>();

            Assert.IsInstanceOfType(output, typeof(TestClassWithInterfaceDependency));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredInterfaceWithUnregisteredInterfaceDependencies_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClassWithInterfaceDependency>();

            var output = container.Resolve<ITestInterface2>();

            Assert.IsInstanceOfType(output, typeof(TestClassWithInterfaceDependency));
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeDefaultCtor_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var result = container.CanResolve<TestClassDefaultCtor>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_UnregisteredTypeDefaultCtor_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            var result = container.CanResolve<TestClassDefaultCtor>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_UnregisteredInterface_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();
            var result = container.CanResolve<ITestInterface>();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredInterface_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var result = container.CanResolve<ITestInterface>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependencies_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependency>();

            var result = container.CanResolve<TestClassWithDependency>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependenciesAndParameters_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.CanResolve<TestClassWithDependencyAndParameters>(new TinyIoC.NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependenciesAndIncorrectParameters_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.CanResolve<TestClassWithDependencyAndParameters>(new TinyIoC.NamedParameterOverloads { { "wrongparam1", 12 }, { "wrongparam2", "Testing" } });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolveType_FactoryRegisteredType_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            var result = container.CanResolve<ITestInterface>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_TinyIoC_ReturnsCurrentContainer()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TinyIoC>();

            Assert.ReferenceEquals(result, container);
        }

        [TestMethod]
        public void Resolve_ClassWithTinyIoCDependency_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithContainerDependency>();

            var result = container.Resolve<TestClassWithContainerDependency>();

            Assert.IsInstanceOfType(result, typeof(TestClassWithContainerDependency));
        }

        [TestMethod]
        public void Register_Instance_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<DisposableTestClassWithInterface>(new DisposableTestClassWithInterface());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InstanceUsingInterface_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>(new DisposableTestClassWithInterface());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_RegisteredInstance_SameInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<DisposableTestClassWithInterface>(item);

            var result = container.Resolve<DisposableTestClassWithInterface>();

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        public void Resolve_RegisteredInstanceWithInterface_SameInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<ITestInterface>(item);

            var result = container.Resolve<ITestInterface>();

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        [Ignore]
        public void Dispose_RegisteredDisposableInstance_CallsDispose()
        {
            var container = UtilityMethods.GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<DisposableTestClassWithInterface>(item);

            container.Dispose();

            Assert.IsTrue(item.Disposed);
        }

        [TestMethod]
        [Ignore]
        public void Dispose_RegisteredDisposableInstanceWithInterface_CallsDispose()
        {
            var container = UtilityMethods.GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<ITestInterface>(item);

            container.Dispose();

            Assert.IsTrue(item.Disposed);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithFluentSingletonCall_ReturnsSingleton()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassNoInterfaceDefaultCtor>().AsSingleton();

            var result = container.Resolve<TestClassNoInterfaceDefaultCtor>();
            var result2 = container.Resolve<TestClassNoInterfaceDefaultCtor>();

            Assert.ReferenceEquals(result, result2);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithInterfaceWithFluentSingletonCall_ReturnsSingleton()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().AsMultiInstance();

            var result = container.Resolve<TestClassNoInterfaceDefaultCtor>();
            var result2 = container.Resolve<TestClassNoInterfaceDefaultCtor>();

            Assert.IsFalse(object.ReferenceEquals(result, result2));
        }

        [TestMethod]
        public void Register_GenericTypeImplementationOnly_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<GenericClassWithInterface<int, string>>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_GenericTypeWithInterface_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface, GenericClassWithInterface<int, string>>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_RegisteredGenericTypeImplementationOnlyCorrectGenericTypes_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<GenericClassWithInterface<int, string>>();

            var result = container.Resolve<GenericClassWithInterface<int, string>>();

            Assert.IsInstanceOfType(result, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        public void Resolve_RegisteredGenericTypeWithInterfaceCorrectGenericTypes_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, GenericClassWithInterface<int, string>>();

            var result = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(result, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        public void Register_NamedRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>("TestName");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_NamedInterfaceRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_NamedInstanceRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();

            container.Register<TestClassDefaultCtor>(item, "TestName");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_NamedFactoryRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>((c, p) => TestClassDefaultCtor.CreateNew(c) as TestClassDefaultCtor, "TestName");

            Assert.IsTrue(true);
        }
    }
}
