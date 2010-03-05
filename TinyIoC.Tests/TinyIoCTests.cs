//===============================================================================
// TinyIoC
//
// An easy to use, hassle free, Inversion of Control Container for small projects
// and beginners alike.
//
// http://hg.grumpydev.com/tinyioc
//===============================================================================
// Copyright © Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

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
            var container = TinyIoCContainer.Current;

            Assert.IsInstanceOfType(container, typeof(TinyIoCContainer));
        }

        [TestMethod]
        public void Current_GetTwice_ReturnsSameInstance()
        {
            var container1 = TinyIoCContainer.Current;
            var container2 = TinyIoCContainer.Current;

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

            var result = container.CanResolve<TestClassWithDependencyAndParameters>(new NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndParameters_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.Resolve<TestClassWithDependencyAndParameters>(new NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.IsInstanceOfType(result, typeof(TestClassWithDependencyAndParameters));
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndParameters_ResolvesWithCorrectConstructor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.Resolve<TestClassWithDependencyAndParameters>(new NamedParameterOverloads { { "param1", 12 }, { "param2", "Testing" } });

            Assert.AreEqual(result.Param1, 12);
            Assert.AreEqual(result.Param2, "Testing");
        }

        [TestMethod]
        public void CanResolveType_RegisteredTypeWithRegisteredDependenciesAndIncorrectParameters_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.CanResolve<TestClassWithDependencyAndParameters>(new NamedParameterOverloads { { "wrongparam1", 12 }, { "wrongparam2", "Testing" } });

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndIncorrectParameters_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            var result = container.Resolve<TestClassWithDependencyAndParameters>(new NamedParameterOverloads { { "wrongparam1", 12 }, { "wrongparam2", "Testing" } });

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_NullFactory_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            Func<TinyIoCContainer, NamedParameterOverloads, ITestInterface> factory = null;
            container.Register<ITestInterface>(factory);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_TinyIoC_ReturnsCurrentContainer()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TinyIoCContainer>();

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
        public void Resolve_RegisteredTypeWithInterfaceWithFluentMultiInstanceCall_ReturnsMultipleInstances()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().AsMultiInstance();

            var result = container.Resolve<TestClassNoInterfaceDefaultCtor>();
            var result2 = container.Resolve<TestClassNoInterfaceDefaultCtor>();

            Assert.IsFalse(object.ReferenceEquals(result, result2));
        }

        [TestMethod]
        public void Resolve_RegisteredInstanceWithFluentMultiInstanceCall_ReturnsMultipleInstance()
        {
            var container = UtilityMethods.GetContainer();
            var input = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>(input).AsMultiInstance();

            var result = container.Resolve<TestClassDefaultCtor>();

            Assert.IsFalse(object.ReferenceEquals(result, input));
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

            var output = container.Resolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.AttemptResolve });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NoNameButOnlyNamedRegistered_ThrowsExceptionWithNoAttemptResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("Testing");

            var output = container.Resolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NamedButOnlyUnnamedRegistered_ThrowsExceptionWithNoFallback()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>("Testing", new ResolveOptions() { NamedResolutionFailureAction =  NamedResolutionFailureActions.Fail });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void Resolve_NamedButOnlyUnnamedRegistered_ResolvesWithFallbackEnabled()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var output = container.Resolve<TestClassDefaultCtor>("Testing", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_CorrectlyRegisteredSpecifyingMistypedParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var output = container.Resolve<TestClassWithParameters>(
                    new NamedParameterOverloads { { "StringProperty", "Testing" }, { "IntProperty", 12 } }
                );

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void Resolve_CorrectlyRegisteredSpecifyingParameters_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var output = container.Resolve<TestClassWithParameters>(
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } }
                );

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void Resolve_CorrectlyRegisteredSpecifyingParametersAndOptions_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var output = container.Resolve<TestClassWithParameters>(
                    new NamedParameterOverloads {{ "stringProperty", "Testing" }, { "intProperty", 12 }},
                    ResolveOptions.Default
                );

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void CanResolve_UnRegisteredType_TrueWithAttemptResolve()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.AttemptResolve });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_UnRegisteredType_FalseWithAttemptResolveOff()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolve_UnRegisteredTypeEithParameters_TrueWithAttemptResolve()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassWithParameters>(
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } },
                    new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.AttemptResolve }
                );

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_UnRegisteredTypeWithParameters_FalseWithAttemptResolveOff()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassWithParameters>(
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } },
                    new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail }
                );

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolve_NamedTypeAndNamedRegistered_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("TestName");

            var result = container.CanResolve<TestClassDefaultCtor>("TestName");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_NamedTypeAndUnnamedRegistered_ReturnsTrueWithFallback()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var result = container.CanResolve<TestClassDefaultCtor>("TestName", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_NamedTypeAndUnnamedRegistered_ReturnsFalseWithFallbackOff()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            var result = container.CanResolve<TestClassDefaultCtor>("TestName", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.Fail });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolve_NamedTypeWithParametersAndNamedRegistered_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("TestName");

            var result = container.CanResolve<TestClassWithParameters>("TestName",
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } }
                );

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_NamedTypeWithParametersAndUnnamedRegistered_ReturnsTrueWithFallback()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var result = container.CanResolve<TestClassWithParameters>("TestName",
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } },
                    new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution }
                );

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_NamedTypeWithParametersAndUnnamedRegistered_ReturnsFalseWithFallbackOff()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            var result = container.CanResolve<TestClassWithParameters>("TestName",
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } },
                    new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.Fail }
                );

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithNameParametersAndOptions_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("TestName");

            var result = container.Resolve<TestClassWithParameters>("TestName",
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } },
                    new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.Fail }
                );

            Assert.IsInstanceOfType(result, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void Resolve_RegisteredTypeWithNameAndParameters_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("TestName");

            var result = container.Resolve<TestClassWithParameters>("TestName",
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } }
                );

            Assert.IsInstanceOfType(result, typeof(TestClassWithParameters));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_ClassWithNoPublicConstructor_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassPrivateCtor>();

            var result = container.Resolve<TestClassPrivateCtor>();

            Assert.IsInstanceOfType(result, typeof(TestClassPrivateCtor));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredSingletonWithParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var output = container.Resolve<ITestInterface>(new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } });

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_WithNullParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            NamedParameterOverloads parameters = null;

            var output = container.Resolve<TestClassDefaultCtor>(parameters);

            Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }


        [TestMethod]
        public void Register_MultiInstanceToSingletonFluent_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>().AsSingleton();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_MultiInstanceToMultiInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>().AsMultiInstance();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_MultiInstanceWithStrongReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>().WithStrongReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_MultiInstanceWithWeakReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>().WithWeakReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_SingletonToSingletonFluent_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().AsSingleton();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_SingletonToMultiInstanceFluent_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().AsMultiInstance();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_SingletonWithStrongReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().WithStrongReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_SingletonWithWeakReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().WithWeakReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_FactoryToSingletonFluent_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>((c,p)=>new TestClassDefaultCtor()).AsSingleton();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_FactoryToMultiInstanceFluent_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).AsMultiInstance();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_FactoryWithStrongReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).WithStrongReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_FactoryWithWeakReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).WithWeakReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_InstanceToSingletonFluent_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).AsSingleton();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InstanceToMultiInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).AsMultiInstance();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InstanceWithStrongReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).WithStrongReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InstanceWithWeakReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).WithWeakReference();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_OutOfScopeStrongReferencedInstance_ResolvesCorrectly()
        {
            var container = UtilityMethods.GetContainer();
            UtilityMethods.RegisterInstanceStrongRef(container);

            GC.Collect();
            GC.WaitForFullGCComplete(4000);

            var result = container.Resolve<TestClassDefaultCtor>();
            Assert.AreEqual("Testing", result.Prop1);
        }


        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_OutOfScopeWeakReferencedInstance_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            UtilityMethods.RegisterInstanceWeakRef(container);

            GC.Collect();
            GC.WaitForFullGCComplete(4000);

            var result = container.Resolve<TestClassDefaultCtor>();
            Assert.AreEqual("Testing", result.Prop1);
        }

        [TestMethod]
        public void Resolve_OutOfScopeStrongReferencedFactory_ResolvesCorrectly()
        {
            var container = UtilityMethods.GetContainer();
            UtilityMethods.RegisterFactoryStrongRef(container);

            GC.Collect();
            GC.WaitForFullGCComplete(4000);

            var result = container.Resolve<TestClassDefaultCtor>();
            Assert.AreEqual("Testing", result.Prop1);
        }


        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_OutOfScopeWeakReferencedFactory_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            UtilityMethods.RegisterFactoryWeakRef(container);

            GC.Collect();
            GC.WaitForFullGCComplete(4000);

            var result = container.Resolve<TestClassDefaultCtor>();
            Assert.AreEqual("Testing", result.Prop1);
        }

        [TestMethod]
        public void Register_InterfaceAndImplementationWithInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InterfaceAndImplementationNamedWithInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            var item2 = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item, "TestName");
            container.Register<ITestInterface, TestClassDefaultCtor>(item2);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolved_InterfaceAndImplementationWithInstance_ReturnsCorrectInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item);

            var result = container.Resolve<ITestInterface>();

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        public void Resolve_InterfaceAndImplementationNamedWithInstance_ReturnsCorrectInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            var item2 = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item, "TestName");
            container.Register<ITestInterface, TestClassDefaultCtor>(item2);

            var result = container.Resolve<ITestInterface>("TestName");

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithoutRegistered_ResolvesWithDefaultOptions()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(GenericClassWithInterface<,>));

            var testing = container.Resolve<GenericClassWithInterface<int, string>>();

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_BoundGenericTypeWithoutRegistered_FailsWithUnRegisteredFallbackOff()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(GenericClassWithInterface<,>));

            var testing = container.Resolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithoutRegistered_ResolvesWithUnRegisteredFallbackSetToGenericsOnly()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(GenericClassWithInterface<,>));

            var testing = container.Resolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly });

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        public void CanResolve_BoundGenericTypeWithoutRegistered_ReturnsTrueWithDefaultOptions()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(GenericClassWithInterface<,>));

            var testing = container.CanResolve<GenericClassWithInterface<int, string>>();

            Assert.IsTrue(testing);
        }

        [TestMethod]
        public void CanResolve_BoundGenericTypeWithoutRegistered_ReturnsFalseWithUnRegisteredFallbackOff()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(GenericClassWithInterface<,>));

            var testing = container.CanResolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsFalse(testing);
        }

        [TestMethod]
        public void CanResolve_BoundGenericTypeWithoutRegistered_ReturnsTrueWithUnRegisteredFallbackSetToGenericsOnly()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(GenericClassWithInterface<,>));

            var testing = container.CanResolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly });

            Assert.IsTrue(testing);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnRegisteredNonGenericType_FailsWithOptionsSetToGenericOnly()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TestClassDefaultCtor>(new ResolveOptions() {UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly});
        
            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void TryResolve_UnRegisteredNonGenericType_ReturnsFalseWithOptionsSetToGenericOnly()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassDefaultCtor>(new ResolveOptions() {UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly});
        
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithParametersWithoutRegistered_ResolvesUsingCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.Resolve<GenericClassWithInterface<int, string>>(new NamedParameterOverloads() { { "prop1", 27 }, { "prop2", "Testing" } });

            Assert.AreEqual(27, testing.Prop1);
            Assert.AreEqual("Testing", testing.Prop2);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_BoundGenericTypeWithFailedDependenciesWithoutRegistered_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.Resolve<GenericClassWithParametersAndDependencies<int, string>>();

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithParametersAndDependencies<int, string>));
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithDependenciesWithoutRegistered_ResolvesUsingCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClass2>();

            var testing = container.Resolve<GenericClassWithParametersAndDependencies<int, string>>();

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithParametersAndDependencies<int, string>));
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithDependenciesAndParametersWithoutRegistered_ResolvesUsingCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClass2>();

            var testing = container.Resolve<GenericClassWithParametersAndDependencies<int, string>>(new NamedParameterOverloads() { { "prop1", 27 }, { "prop2", "Testing" } });

            Assert.AreEqual(27, testing.Prop1);
            Assert.AreEqual("Testing", testing.Prop2);
        }

        [TestMethod]
        public void Resolve_NamedRegistrationButOnlyUnnamedRegistered_ResolvesCorrectUnnamedRegistrationWithUnnamedFallback()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor() { Prop1 = "Testing" };
            container.Register<TestClassDefaultCtor>(item);

            var result = container.Resolve<TestClassDefaultCtor>("Testing",new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.ReferenceEquals(item, result);
        }

        [TestMethod]
        public void Resolve_ClassWithLazyFactoryDependency_Resolves()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TestClassWithLazyFactory>();

            Assert.IsInstanceOfType(result, typeof(TestClassWithLazyFactory));
        }

        [TestMethod]
        public void CanResolve_ClassWithLazyFactoryDependency_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassWithLazyFactory>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Resolve_ClassWithNamedLazyFactoryDependency_Resolves()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TestClassWithNamedLazyFactory>();

            Assert.IsInstanceOfType(result, typeof(TestClassWithNamedLazyFactory));
        }

        [TestMethod]
        public void CanResolve_ClassNamedWithLazyFactoryDependency_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassWithNamedLazyFactory>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LazyFactory_CalledByDependantClass_ReturnsInstanceOfType()
        {
            var container = UtilityMethods.GetContainer();
            var item = container.Resolve<TestClassWithLazyFactory>();

            item.Method1();

            Assert.IsInstanceOfType(item.Prop1, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void NamedLazyFactory_CalledByDependantClass_ReturnsCorrectInstanceOfType()
        {
            var container = UtilityMethods.GetContainer();
            var item1 = new TestClassDefaultCtor();
            var item2 = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>(item1, "Testing");
            container.Register<TestClassDefaultCtor>(item2);
            container.Register<TestClassWithNamedLazyFactory>();

            var item = container.Resolve<TestClassWithNamedLazyFactory>();

            item.Method1();
            item.Method2();

            Assert.ReferenceEquals(item.Prop1, item1);
            Assert.ReferenceEquals(item.Prop2, item2);
        }

        [TestMethod]
        public void AutoRegister_NoParameters_ReturnsNoErrors()
        {
            var container = UtilityMethods.GetContainer();

            container.AutoRegister();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void AutoRegister_AssemblySpecified_ReturnsNoErrors()
        {
            var container = UtilityMethods.GetContainer();

            container.AutoRegister(this.GetType().Assembly);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void AutoRegister_TestAssembly_CanResolveInterface()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(this.GetType().Assembly);

            var result = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void AutoRegister_TinyIoCAssembly_CannotResolveInternalTinyIoCClass()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(container.GetType().Assembly);

            var output = container.Resolve<TinyIoCContainer.TypeRegistration>(new NamedParameterOverloads() { { "type", this.GetType() } }, new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsInstanceOfType(output, typeof(TinyIoCContainer.TypeRegistration));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForDelegateFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).UsingConstructor(() => new TestClassDefaultCtor());

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForWeakDelegateFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).WithWeakReference().UsingConstructor(() => new TestClassDefaultCtor());

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForInstanceFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).UsingConstructor(() => new TestClassDefaultCtor());

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForWeakInstanceFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).WithWeakReference().UsingConstructor(() => new TestClassDefaultCtor());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_ConstructorSpecifiedForMultiInstanceFactory_Registers()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>().UsingConstructor(() => new TestClassDefaultCtor());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_ConstructorSpecifiedForSingletonFactory_Registers()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface, TestClassDefaultCtor>().UsingConstructor(() => new TestClassDefaultCtor());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_SingletonFactoryConstructorSpecified_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassMultiDepsMultiCtors>().AsSingleton().UsingConstructor(() => new TestClassMultiDepsMultiCtors(null as TestClassDefaultCtor));

            var result = container.Resolve<TestClassMultiDepsMultiCtors>();

            Assert.AreEqual(1, result.NumberOfDepsResolved);
        }

        [TestMethod]
        public void Resolve_MultiInstanceFactoryConstructorSpecified_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassMultiDepsMultiCtors>().UsingConstructor(() => new TestClassMultiDepsMultiCtors(null as TestClassDefaultCtor));

            var result = container.Resolve<TestClassMultiDepsMultiCtors>();

            Assert.AreEqual(1, result.NumberOfDepsResolved);
        }

        [TestMethod]
        public void Resolve_SingletonFactoryNoConstructorSpecified_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassMultiDepsMultiCtors>().AsSingleton();

            var result = container.Resolve<TestClassMultiDepsMultiCtors>();

            Assert.AreEqual(2, result.NumberOfDepsResolved);
        }

        [TestMethod]
        public void Resolve_MultiInstanceFactoryNoConstructorSpecified_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassMultiDepsMultiCtors>();

            var result = container.Resolve<TestClassMultiDepsMultiCtors>();

            Assert.AreEqual(2, result.NumberOfDepsResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_ConstructorSpecifiedThatRequiresParametersButNonePassed_FailsToResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<TestClassWithInterfaceDependency>().UsingConstructor(() => new TestClassWithInterfaceDependency(null as ITestInterface, 27, "Testing"));

            var result = container.Resolve<TestClassWithInterfaceDependency>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void CanResolve_ConstructorSpecifiedThatRequiresParametersButNonePassed_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<TestClassWithInterfaceDependency>().UsingConstructor(() => new TestClassWithInterfaceDependency(null as ITestInterface, 27, "Testing"));

            var result = container.CanResolve<TestClassWithInterfaceDependency>();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanResolve_SingletonFactoryConstructorSpecified_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassMultiDepsMultiCtors>().AsSingleton().UsingConstructor(() => new TestClassMultiDepsMultiCtors(null as TestClassDefaultCtor));

            var result = container.CanResolve<TestClassMultiDepsMultiCtors>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolve_MultiInstanceFactoryConstructorSpecified_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassMultiDepsMultiCtors>().UsingConstructor(() => new TestClassMultiDepsMultiCtors(null as TestClassDefaultCtor));

            var result = container.CanResolve<TestClassMultiDepsMultiCtors>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_ConstructorThrowsException_ThrowsTinyIoCException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassConstructorFailure>();

            var result = container.Resolve<TestClassConstructorFailure>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_DelegateFactoryThrowsException_ThrowsTinyIoCException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassConstructorFailure>((c, p) => { throw new NotImplementedException(); });

            var result = container.Resolve<TestClassConstructorFailure>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_DelegateFactoryResolvedWithUnnamedFallbackThrowsException_ThrowsTinyIoCException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassConstructorFailure>((c, p) => { throw new NotImplementedException(); });

            var result = container.Resolve<TestClassConstructorFailure>("Testing", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationTypeExceptiopn))]
        public void Register_AbstractClassWithNoImplementation_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassBase>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCRegistrationTypeExceptiopn))]
        public void Register_InterfaceWithNoImplementation_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface>();

            Assert.IsTrue(true);
        }
    }
}
