using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using TinyIoC.Tests.TestData;
using TinyIoC.Tests.TestData.BasicClasses;
using Moq;
using NestedInterfaceDependencies = TinyIoC.Tests.TestData.NestedInterfaceDependencies;
using NestedClassDependencies = TinyIoC.Tests.TestData.NestedClassDependencies;

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
        public void Resolve_RegisteredTypeWithRegisteredDependencies_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependency>();

            var result = container.Resolve<TestClassWithDependency>();

            Assert.IsInstanceOfType(result, typeof(TestClassWithDependency));
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
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndParameters_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.Resolve<TestClassWithDependencyAndParameters>(new TinyIoC.NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.IsInstanceOfType(result, typeof(TestClassWithDependencyAndParameters));
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndParameters_ResolvesWithCorrectConstructor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.Resolve<TestClassWithDependencyAndParameters>(new TinyIoC.NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.AreEqual(result.Param1, 12);
            Assert.AreEqual(result.Param2, "Testing");
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
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndIncorrectParameters_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.Resolve<TestClassWithDependencyAndParameters>(new TinyIoC.NamedParameterOverloads { { "wrongparam1", 12 }, { "wrongparam2", "Testing" } });

            Assert.IsInstanceOfType(result, typeof(TestClassWithDependencyAndParameters));
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
        public void CanResolveType_FactoryRegisteredTypeThatThrows_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => { throw new NotImplementedException(); });

            var result = container.CanResolve<ITestInterface>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_FactoryRegisteredTypeThatThrows_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => { throw new NotImplementedException(); });

            var result = container.Resolve<ITestInterface>();

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
            var item = new Mock<DisposableTestClassWithInterface>();
            var disposableItem = item.As<IDisposable>();
            disposableItem.Setup(i => i.Dispose());

            var container = UtilityMethods.GetContainer();
            container.Register<DisposableTestClassWithInterface>(item.Object);

            container.Dispose();

            item.VerifyAll();
        }

        [TestMethod]
        [Ignore]
        public void Dispose_RegisteredDisposableInstanceWithInterface_CallsDispose()
        {
            var item = new Mock<DisposableTestClassWithInterface>();
            var disposableItem = item.As<IDisposable>();
            disposableItem.Setup(i => i.Dispose());

            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>(item.Object);

            container.Dispose();

            item.VerifyAll();
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

        [TestMethod]
        public void Resolve_NamedRegistrationFollowedByNormal_CanResolveNamed()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("TestName");
            container.Register<TestClassDefaultCtor>();

            var result = container.Resolve<TestClassDefaultCtor>("TestName");

            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_NormalRegistrationFollowedByNamed_CanResolveNormal()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassDefaultCtor>("TestName");

            var result = container.Resolve<TestClassDefaultCtor>();

            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_NamedInterfaceRegistrationFollowedByNormal_CanResolveNamed()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var result = container.Resolve<ITestInterface>("TestName");

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void Resolve_NormalInterfaceRegistrationFollowedByNamed_CanResolveNormal()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");

            var result = container.Resolve<TestClassDefaultCtor>();

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void Resolve_NamedInstanceRegistrationFollowedByNormal_CanResolveNamed()
        {
            var container = UtilityMethods.GetContainer();
            var instance1 = new TestClassDefaultCtor();
            var instance2 = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>(instance1, "TestName");
            container.Register<TestClassDefaultCtor>(instance2);

            var result = container.Resolve<TestClassDefaultCtor>("TestName");

            Assert.ReferenceEquals(instance1, result);
        }

        [TestMethod]
        public void Resolve_NormalInstanceRegistrationFollowedByNamed_CanResolveNormal()
        {
            var container = UtilityMethods.GetContainer();
            var instance1 = new TestClassDefaultCtor();
            var instance2 = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>(instance1);
            container.Register<TestClassDefaultCtor>(instance2, "TestName");

            var result = container.Resolve<TestClassDefaultCtor>();

            Assert.ReferenceEquals(instance1, result);
        }


        [TestMethod]
        public void Resolve_NamedFactoryRegistrationFollowedByNormal_CanResolveNamed()
        {
            var container = UtilityMethods.GetContainer();
            var instance1 = new TestClassDefaultCtor();
            var instance2 = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>((c,p) => instance1, "TestName");
            container.Register<TestClassDefaultCtor>((c,p) => instance2);

            var result = container.Resolve<TestClassDefaultCtor>("TestName");

            Assert.ReferenceEquals(instance1, result);
        }

        [TestMethod]
        public void Resolve_FactoryInstanceRegistrationFollowedByNamed_CanResolveNormal()
        {
            var container = UtilityMethods.GetContainer();
            var instance1 = new TestClassDefaultCtor();
            var instance2 = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>((c, p) => instance1);
            container.Register<TestClassDefaultCtor>((c, p) => instance2, "TestName");

            var result = container.Resolve<TestClassDefaultCtor>();

            Assert.ReferenceEquals(instance1, result);
        }

        [TestMethod]
        public void Resolve_NoNameButOnlyNamedRegistered_ResolvesWithAttemptResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("Testing");

            var output = container.Resolve<TestClassDefaultCtor>(new TinyIoC.ResolveOptions() { UnregisteredResolutionAction = TinyIoC.ResolveOptions.UnregisteredResolutionActions.AttemptResolve });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NoNameButOnlyNamedRegistered_ThrowsExceptionWithNoAttemptResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("Testing");

            var output = container.Resolve<TestClassDefaultCtor>(new TinyIoC.ResolveOptions() { UnregisteredResolutionAction = TinyIoC.ResolveOptions.UnregisteredResolutionActions.Fail });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NamedButOnlyUnnamedRegistered_ThrowsExceptionWithNoFallback()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>("Testing", new TinyIoC.ResolveOptions() { NamedResolutionFailureAction =  TinyIoC.ResolveOptions.NamedResolutionFailureActions.Fail });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_NamedButOnlyUnnamedRegistered_ResolvesWithFallbackEnabled()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>("Testing", new TinyIoC.ResolveOptions() { NamedResolutionFailureAction = TinyIoC.ResolveOptions.NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_CorrectlyRegisteredSpecifyingMistypedParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var output = container.Resolve<TestClassWithParameters>(
                    new TinyIoC.NamedParameterOverloads { { "StringProperty", "Testing" }, { "IntProperty", 12 } }
                );

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void Resolve_CorrectlyRegisteredSpecifyingParameters_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var output = container.Resolve<TestClassWithParameters>(
                    new TinyIoC.NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } }
                );

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void Resolve_CorrectlyRegisteredSpecifyingParametersAndOptions_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var output = container.Resolve<TestClassWithParameters>(
                    new TinyIoC.NamedParameterOverloads {{ "stringProperty", "Testing" }, { "intProperty", 12 }},
                    TinyIoC.ResolveOptions.GetDefault()
                );

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        #region Scenario Tests
        [TestMethod]
        public void NestedInterfaceDependencies_CorrectlyRegistered_ResolvesRoot()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedInterfaceDependencies.IService1, NestedInterfaceDependencies.Service1>();
            container.Register<NestedInterfaceDependencies.IService2, NestedInterfaceDependencies.Service2>();
            container.Register<NestedInterfaceDependencies.IService3, NestedInterfaceDependencies.Service3>();
            container.Register<NestedInterfaceDependencies.RootClass>();

            var result = container.Resolve<NestedInterfaceDependencies.RootClass>();

            Assert.IsInstanceOfType(result, typeof(NestedInterfaceDependencies.RootClass));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void NestedInterfaceDependencies_MissingIService3Registration_ThrowsExceptionWithDefaultSettings()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedInterfaceDependencies.IService1, NestedInterfaceDependencies.Service1>();
            container.Register<NestedInterfaceDependencies.IService2, NestedInterfaceDependencies.Service2>();
            container.Register<NestedInterfaceDependencies.RootClass>();

            var result = container.Resolve<NestedInterfaceDependencies.RootClass>();

            Assert.IsInstanceOfType(result, typeof(NestedInterfaceDependencies.RootClass));
        }

        [TestMethod]
        public void NestedClassDependencies_CorrectlyRegistered_ResolvesRoot()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedClassDependencies.Service1>();
            container.Register<NestedClassDependencies.Service2>();
            container.Register<NestedClassDependencies.Service3>();
            container.Register<NestedClassDependencies.RootClass>();

            var result = container.Resolve<NestedClassDependencies.RootClass>();

            Assert.IsInstanceOfType(result, typeof(NestedClassDependencies.RootClass));
        }

        [TestMethod]
        public void NestedClassDependencies_MissingService3Registration_ResolvesRootWithDefaultSettings()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedClassDependencies.Service1>();
            container.Register<NestedClassDependencies.Service2>();
            container.Register<NestedClassDependencies.RootClass>();

            var result = container.Resolve<NestedClassDependencies.RootClass>();

            Assert.IsInstanceOfType(result, typeof(NestedClassDependencies.RootClass));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void NestedClassDependencies_MissingService3RegistrationAndUnRegisteredResolutionOff_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedClassDependencies.Service1>();
            container.Register<NestedClassDependencies.Service2>();
            container.Register<NestedClassDependencies.RootClass>();

            var result = container.Resolve<NestedClassDependencies.RootClass>(new TinyIoC.ResolveOptions() { UnregisteredResolutionAction = TinyIoC.ResolveOptions.UnregisteredResolutionActions.Fail });

            Assert.IsInstanceOfType(result, typeof(NestedClassDependencies.RootClass));
        }
        #endregion

    }
}
