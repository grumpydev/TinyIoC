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
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using TinyIoC.Tests.TestData;
using TinyIoC.Tests.TestData.BasicClasses;
using NestedInterfaceDependencies = TinyIoC.Tests.TestData.NestedInterfaceDependencies;
using NestedClassDependencies = TinyIoC.Tests.TestData.NestedClassDependencies;

#if !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

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

            Assert.AreSame(container1, container2);
        }

        [TestMethod]
        public void Register_ImplementationOnly_CanRegister()
        {
            UtilityMethods.GetContainer().Register<TestClassDefaultCtor>();
        }

        [TestMethod]
        public void Register_InterfaceAndImplementation_CanRegister()
        {
            UtilityMethods.GetContainer().Register<ITestInterface, TestClassDefaultCtor>();
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

            Assert.AreSame(output, output2);
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
        }

        [TestMethod]
        public void Register_WithDelegateFactoryLambda_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => new TestClassDefaultCtor() { Prop1 = "Testing" });
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnregisteredInterface_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<ITestInterface>());

            //Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor)));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnregisteredClassWithUnregisteredInterfaceDependencies_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassWithInterfaceDependency>());

            //Assert.IsInstanceOfType(output, typeof(TestClassWithInterfaceDependency)));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredClassWithUnregisteredInterfaceDependencies_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithInterfaceDependency>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassWithInterfaceDependency>());

            //Assert.IsInstanceOfType(output, typeof(TestClassWithInterfaceDependency)));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredInterfaceWithUnregisteredInterfaceDependencies_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClassWithInterfaceDependency>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<ITestInterface2>());

            //Assert.IsInstanceOfType(output, typeof(TestClassWithInterfaceDependency)));
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredTypeWithRegisteredDependenciesAndIncorrectParameters_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            container.Register<TestClassWithDependencyAndParameters>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassWithDependencyAndParameters>(new NamedParameterOverloads { { "wrongparam1", 12 }, { "wrongparam2", "Testing" } }));

            //Assert.IsInstanceOfType(result, typeof(TestClassWithDependencyAndParameters));
        }

        [TestMethod]
        public void CanResolveType_FactoryRegisteredType_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => TestClassDefaultCtor.CreateNew(c));

            var result = container.CanResolve<ITestInterface>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanResolveType_FactoryRegisteredTypeThatThrows_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => { throw new NotImplementedException(); });

            var result = container.CanResolve<ITestInterface>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_FactoryRegisteredTypeThatThrows_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>((c, p) => { throw new NotImplementedException(); });

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<ITestInterface>());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void Register_NullFactory_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            Func<TinyIoCContainer, NamedParameterOverloads, ITestInterface> factory = null;
            AssertHelper.ThrowsException<ArgumentNullException>(() => container.Register<ITestInterface>(factory));

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void Resolve_TinyIoC_ReturnsCurrentContainer()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TinyIoCContainer>();

            Assert.AreSame(result, container);
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
        }

        [TestMethod]
        public void Register_InstanceUsingInterface_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface>(new DisposableTestClassWithInterface());
        }

        [TestMethod]
        public void Resolve_RegisteredInstance_SameInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<DisposableTestClassWithInterface>(item);

            var result = container.Resolve<DisposableTestClassWithInterface>();

            Assert.AreSame(item, result);
        }

        [TestMethod]
        public void Resolve_RegisteredInstanceWithInterface_SameInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new DisposableTestClassWithInterface();
            container.Register<ITestInterface>(item);

            var result = container.Resolve<ITestInterface>();

            Assert.AreSame(item, result);
        }

#if MOQ
        [TestMethod]
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
#endif

#if MOQ
        [TestMethod]
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
#endif

        [TestMethod]
        public void Resolve_RegisteredTypeWithFluentSingletonCall_ReturnsSingleton()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassNoInterfaceDefaultCtor>().AsSingleton();

            var result = container.Resolve<TestClassNoInterfaceDefaultCtor>();
            var result2 = container.Resolve<TestClassNoInterfaceDefaultCtor>();

            Assert.AreSame(result, result2);
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
        }

        [TestMethod]
        public void Register_GenericTypeWithInterface_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface, GenericClassWithInterface<int, string>>();
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
        public void Resolve_RegisteredGenericTypeWithGenericInterfaceCorrectGenericTypes_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface<int, string>, GenericClassWithGenericInterface<int, string>>();

            var result = container.Resolve<ITestInterface<int, string>>();

            Assert.IsInstanceOfType(result, typeof(GenericClassWithGenericInterface<int, string>));
        }

        [TestMethod]
        public void Resolve_RegisteredGenericTypeWithGenericInterfaceCorrectGenericTypesWithParent_Resolves()
        {
            var container = UtilityMethods.GetContainer();
            var child = container.GetChildContainer();
            container.Register<ITestInterface<int, string>>(new GenericClassWithGenericInterface<int, string>());

            var result = child.Resolve<ITestInterface<int, string>>();

            Assert.IsInstanceOfType(result, typeof(GenericClassWithGenericInterface<int, string>));
        }


        [TestMethod]
        public void Register_NamedRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>("TestName");
        }

        [TestMethod]
        public void Register_NamedInterfaceRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");
        }

        [TestMethod]
        public void Register_NamedInstanceRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();

            container.Register<TestClassDefaultCtor>(item, "TestName");
        }

        [TestMethod]
        public void Register_NamedFactoryRegistration_CanRegister()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>((c, p) => TestClassDefaultCtor.CreateNew(c) as TestClassDefaultCtor, "TestName");
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

            Assert.AreSame(instance1, result);
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

            Assert.AreSame(instance1, result);
        }


        [TestMethod]
        public void Resolve_NamedFactoryRegistrationFollowedByNormal_CanResolveNamed()
        {
            var container = UtilityMethods.GetContainer();
            var instance1 = new TestClassDefaultCtor();
            var instance2 = new TestClassDefaultCtor();
            container.Register<TestClassDefaultCtor>((c, p) => instance1, "TestName");
            container.Register<TestClassDefaultCtor>((c, p) => instance2);

            var result = container.Resolve<TestClassDefaultCtor>("TestName");

            Assert.AreSame(instance1, result);
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

            Assert.AreSame(instance1, result);
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NoNameButOnlyNamedRegistered_ThrowsExceptionWithNoAttemptResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("Testing");

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail }));

            //Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NamedButOnlyUnnamedRegistered_ThrowsExceptionWithNoFallback()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassDefaultCtor>("Testing", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.Fail }));

            //Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_CorrectlyRegisteredSpecifyingMistypedParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassWithParameters>(
                    new NamedParameterOverloads { { "StringProperty", "Testing" }, { "IntProperty", 12 } }
                ));

            //Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
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
                    new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } },
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_ClassWithNoPublicConstructor_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassPrivateCtor>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassPrivateCtor>());

            //Assert.IsInstanceOfType(result, typeof(TestClassPrivateCtor));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_RegisteredSingletonWithParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<ITestInterface>(new NamedParameterOverloads { { "stringProperty", "Testing" }, { "intProperty", 12 } }));

            //Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_WithNullParameters_ThrowsCorrectException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();
            NamedParameterOverloads parameters = null;

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassDefaultCtor>(parameters));

            //Assert.IsInstanceOfType(output, typeof(TestClassDefaultCtor));
        }


        [TestMethod]
        public void Register_MultiInstanceToSingletonFluent_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>().AsSingleton();
        }

        [TestMethod]
        public void Register_MultiInstanceToMultiInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>().AsMultiInstance();
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_MultiInstanceWithStrongReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<TestClassDefaultCtor>().WithStrongReference());
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_MultiInstanceWithWeakReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<TestClassDefaultCtor>().WithWeakReference());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void Register_SingletonToSingletonFluent_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().AsSingleton();
        }

        [TestMethod]
        public void Register_SingletonToMultiInstanceFluent_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>().AsMultiInstance();
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_SingletonWithStrongReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<ITestInterface, TestClassDefaultCtor>().WithStrongReference());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_SingletonWithWeakReference_Throws()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<ITestInterface, TestClassDefaultCtor>().WithWeakReference());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_FactoryToSingletonFluent_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).AsSingleton());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_FactoryToMultiInstanceFluent_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).AsMultiInstance());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void Register_FactoryWithStrongReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).WithStrongReference();
        }

        [TestMethod]
        public void Register_FactoryWithWeakReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).WithWeakReference();
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationException))]
        public void Register_InstanceToSingletonFluent_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            AssertHelper.ThrowsException<TinyIoCRegistrationException>(() => container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).AsSingleton());
        }

        [TestMethod]
        public void Register_InstanceToMultiInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).AsMultiInstance();
        }

        [TestMethod]
        public void Register_InstanceWithStrongReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).WithStrongReference();
        }

        [TestMethod]
        public void Register_InstanceWithWeakReference_Registers()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).WithWeakReference();
        }

// @mbrit - 2012-05-22 - forced GC not supported in WinRT...
#if !NETFX_CORE
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
#endif

// @mbrit - 2012-05-22 - forced GC not supported in WinRT...
#if !NETFX_CORE
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
#endif

// @mbrit - 2012-05-22 - forced GC not supported in WinRT...
#if !NETFX_CORE
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
#endif

// @mbrit - 2012-05-22 - forced GC not supported in WinRT...
#if !NETFX_CORE
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
#endif

        [TestMethod]
        public void Register_InterfaceAndImplementationWithInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item);
        }

        [TestMethod]
        public void Register_InterfaceAndImplementationNamedWithInstance_Registers()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            var item2 = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item, "TestName");
            container.Register<ITestInterface, TestClassDefaultCtor>(item2);
        }

        [TestMethod]
        public void Resolved_InterfaceAndImplementationWithInstance_ReturnsCorrectInstance()
        {
            var container = UtilityMethods.GetContainer();
            var item = new TestClassDefaultCtor();
            container.Register<ITestInterface, TestClassDefaultCtor>(item);

            var result = container.Resolve<ITestInterface>();

            Assert.AreSame(item, result);
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

            Assert.AreSame(item, result);
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithoutRegistered_ResolvesWithDefaultOptions()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.Resolve<GenericClassWithInterface<int, string>>();

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_BoundGenericTypeWithoutRegistered_FailsWithUnRegisteredFallbackOff()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail }));

            //Assert.IsInstanceOfType(testing, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_NormalUnregisteredType_FailsWithUnregisteredFallbackSetToGenericsOnly()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly }));
        }

        [TestMethod]
        public void Resolve_BoundGenericTypeWithoutRegistered_ResolvesWithUnRegisteredFallbackSetToGenericsOnly()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.Resolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly });

            Assert.IsInstanceOfType(testing, typeof(GenericClassWithInterface<int, string>));
        }

        [TestMethod]
        public void CanResolve_BoundGenericTypeWithoutRegistered_ReturnsTrueWithDefaultOptions()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.CanResolve<GenericClassWithInterface<int, string>>();

            Assert.IsTrue(testing);
        }

        [TestMethod]
        public void CanResolve_BoundGenericTypeWithoutRegistered_ReturnsFalseWithUnRegisteredFallbackOff()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.CanResolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsFalse(testing);
        }

        [TestMethod]
        public void CanResolve_BoundGenericTypeWithoutRegistered_ReturnsTrueWithUnRegisteredFallbackSetToGenericsOnly()
        {
            var container = UtilityMethods.GetContainer();

            var testing = container.CanResolve<GenericClassWithInterface<int, string>>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly });

            Assert.IsTrue(testing);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_UnRegisteredNonGenericType_FailsWithOptionsSetToGenericOnly()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly }));

            //assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void CanResolve_UnRegisteredNonGenericType_ReturnsFalseWithOptionsSetToGenericOnly()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestClassDefaultCtor>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.GenericsOnly });

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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_BoundGenericTypeWithFailedDependenciesWithoutRegistered_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<GenericClassWithParametersAndDependencies<int, string>>());

            //Assert.IsInstanceOfType(testing, typeof(GenericClassWithParametersAndDependencies<int, string>));
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

            var result = container.Resolve<TestClassDefaultCtor>("Testing", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.AreSame(item, result);
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

            Assert.AreSame(item.Prop1, item1);
            Assert.AreSame(item.Prop2, item2);
        }

        [TestMethod]
        public void AutoRegister_NoParameters_ReturnsNoErrors()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister();
        }

        [TestMethod]
        public void AutoRegister_AssemblySpecified_ReturnsNoErrors()
        {
            var container = UtilityMethods.GetContainer();

            container.AutoRegister(new[] { this.GetType().Assembly });
        }

        [TestMethod]
        public void AutoRegister_TestAssembly_CanResolveInterface()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { this.GetType().Assembly });

            var result = container.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void AutoRegister_TestAssembly_CanResolveAbstractBaseClass()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { this.GetType().Assembly });

            var result = container.Resolve<TestClassBase>();

            Assert.IsInstanceOfType(result, typeof(TestClassBase));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void AutoRegister_TinyIoCAssembly_CannotResolveInternalTinyIoCClass()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { container.GetType().Assembly });

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TinyIoCContainer.TypeRegistration>(new NamedParameterOverloads() { { "type", this.GetType() } }, new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail }));

            //Assert.IsInstanceOfType(output, typeof(TinyIoCContainer.TypeRegistration));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyIoCAutoRegistrationException))]
        public void AutoRegister_ThisAssemblySpecifiedDuplicateActionFail_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { this.GetType().Assembly }, DuplicateImplementationActions.Fail);
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void AutoRegister_TinyIoCAssemblySpecifiedDuplicateActionFail_NoErrors()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { typeof(TinyIoCContainer).Assembly }, DuplicateImplementationActions.Fail);
        }

        [TestMethod]
        public void AutoRegister_SpecifiedDuplicateActionRegisterMultiple_RegistersMultipleImplementations()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { typeof(TestClassDefaultCtor).Assembly }, DuplicateImplementationActions.RegisterMultiple);
            
            var results = container.ResolveAll<ITestInterface>();

            Assert.IsInstanceOfType(results.First(), typeof(TestClassDefaultCtor));
            Assert.IsInstanceOfType(results.ElementAt(1), typeof(DisposableTestClassWithInterface));
        }

        [TestMethod]
        public void Register_ConstructorSpecifiedForDelegateFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCConstructorResolutionException>(() => container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).UsingConstructor(() => new TestClassDefaultCtor()));

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForWeakDelegateFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCConstructorResolutionException>(() => container.Register<TestClassDefaultCtor>((c, p) => new TestClassDefaultCtor()).WithWeakReference().UsingConstructor(() => new TestClassDefaultCtor()));

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForInstanceFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCConstructorResolutionException>(() => container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).UsingConstructor(() => new TestClassDefaultCtor()));

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCConstructorResolutionException))]
        public void Register_ConstructorSpecifiedForWeakInstanceFactory_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCConstructorResolutionException>(() => container.Register<TestClassDefaultCtor>(new TestClassDefaultCtor()).WithWeakReference().UsingConstructor(() => new TestClassDefaultCtor()));

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void Register_ConstructorSpecifiedForMultiInstanceFactory_Registers()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<TestClassDefaultCtor>().UsingConstructor(() => new TestClassDefaultCtor());
        }

        [TestMethod]
        public void Register_ConstructorSpecifiedForSingletonFactory_Registers()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<ITestInterface, TestClassDefaultCtor>().UsingConstructor(() => new TestClassDefaultCtor());
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_ConstructorSpecifiedThatRequiresParametersButNonePassed_FailsToResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<TestClassWithInterfaceDependency>().UsingConstructor(() => new TestClassWithInterfaceDependency(null as ITestInterface, 27, "Testing"));

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassWithInterfaceDependency>());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void Resolve_ConstructorSpecifiedWithAttribute_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            var result = container.Resolve<TestClassWithConstructorAttrib>();

            Assert.IsTrue(result.AttributeConstructorUsed);
        }

        [TestMethod]
        public void Resolve_InternalConstructorSpecifiedWithAttribute_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            var result = container.Resolve<TestClassWithInternalConstructorAttrib>();

            Assert.IsTrue(result.AttributeConstructorUsed);
        }

        [TestMethod]
        public void Resolve_ManyConstructorsSpecifiedWithAttribute_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            var result = container.Resolve<TestClassWithManyConstructorAttribs>();

            Assert.IsTrue(result.MostGreedyAttribCtorUsed);
        }

        [TestMethod]
        public void Resolve_AttributeConstructorOverriden_UsesCorrectCtor()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithConstructorAttrib>()
                .UsingConstructor(() => new TestClassWithConstructorAttrib(null)); // The non-attribute constructor

            var result = container.Resolve<TestClassWithConstructorAttrib>();

            Assert.IsFalse(result.AttributeConstructorUsed);
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_ConstructorThrowsException_ThrowsTinyIoCException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassConstructorFailure>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassConstructorFailure>());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_DelegateFactoryThrowsException_ThrowsTinyIoCException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassConstructorFailure>((c, p) => { throw new NotImplementedException(); });

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassConstructorFailure>());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void Resolve_DelegateFactoryResolvedWithUnnamedFallbackThrowsException_ThrowsTinyIoCException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassConstructorFailure>((c, p) => { throw new NotImplementedException(); });

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<TestClassConstructorFailure>("Testing", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution }));

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationTypeException))]
        public void Register_AbstractClassWithNoImplementation_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCRegistrationTypeException>(() => container.Register<TestClassBase>());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCRegistrationTypeException))]
        public void Register_InterfaceWithNoImplementation_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();

            AssertHelper.ThrowsException<TinyIoCRegistrationTypeException>(() => container.Register<ITestInterface>());

            // Should have thrown by now
            //Assert.IsTrue(false);
        }

        [TestMethod]
        public void BuildUp_ObjectWithPropertyDependenciesAndDepsRegistered_SetsDependencies()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface2, TestClass2>();
            var input = new TestClassPropertyDependencies();

            container.BuildUp(input);

            Assert.IsNotNull(input.Property1);
            Assert.IsNotNull(input.Property2);
            Assert.IsNotNull(input.ConcreteProperty);
        }

        [TestMethod]
        public void BuildUp_ObjectAndOptionsWithPropertyDependenciesAndDepsRegistered_SetsDependencies()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface2, TestClass2>();
            var input = new TestClassPropertyDependencies();

            container.BuildUp(input, new ResolveOptions());

            Assert.IsNotNull(input.Property1);
            Assert.IsNotNull(input.Property2);
            Assert.IsNotNull(input.ConcreteProperty);
        }

        [TestMethod]
        public void BuildUp_ObjectAndOptionsWithPropertyDependenciesAndDepsRegistered_SetsDependenciesUsingOptions()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface2, TestClass2>();
            var input = new TestClassPropertyDependencies();

            container.BuildUp(input, new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsNotNull(input.Property1);
            Assert.IsNotNull(input.Property2);
            Assert.IsNull(input.ConcreteProperty);
        }

        [TestMethod]
        public void BuildUp_ObjectWithPropertyDependenciesAndDepsNotRegistered_DoesNotThrow()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            var input = new TestClassPropertyDependencies();

            container.BuildUp(input);

            Assert.IsNotNull(input.Property1);
            Assert.IsNull(input.Property2);
        }

        [TestMethod]
        public void BuildUp_ObjectWithPropertyDependenciesWithSomeSet_SetsOnlyUnsetDependencies()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface2, TestClass2>();
            var preset = new TestClassDefaultCtor();
            var input = new TestClassPropertyDependencies();
            input.Property1 = preset;

            container.BuildUp(input);

            Assert.AreSame(preset, input.Property1);
            Assert.IsNotNull(input.Property2);
        }

        [TestMethod]
        public void BuildUp_ObjectWithPropertyDependenciesAndDepsRegistered_DoesNotSetWriteOnlyProperty()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface2, TestClass2>();
            var input = new TestClassPropertyDependencies();

            container.BuildUp(input);

            Assert.IsNull(input.WriteOnlyProperty);
        }

        [TestMethod]
        public void Resolve_ClassWithNameAndParamsLazyFactoryDependency_Resolves()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TestclassWithNameAndParamsLazyFactory>();

            Assert.IsInstanceOfType(result, typeof(TestclassWithNameAndParamsLazyFactory));
        }

        [TestMethod]
        public void CanResolve_ClassWithNameAndParamsLazyFactoryDependency_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.CanResolve<TestclassWithNameAndParamsLazyFactory>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void NameAndParamsLazyFactoryInvoke_Params_ResolvesWithParameters()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.Resolve<TestclassWithNameAndParamsLazyFactory>();

            // Values should be set by the ctor of TestclassWithNameAndParamsLazyFactory
            Assert.AreEqual(result.Prop1.StringProperty, "Testing");
            Assert.AreEqual(result.Prop1.IntProperty, 22);
        }

        [TestMethod]
        public void NamedParameterOverloads_ConstructedUsingFromIDictionary_CopiesDictionary()
        {
            var dictionary = new Dictionary<string, object>() { { "Test", "Test" } };

            var output = NamedParameterOverloads.FromIDictionary(dictionary);

            Assert.IsNotNull(output);
            Assert.AreEqual("Test", output["Test"]);
        }

        [TestMethod]
        public void AutoRegister_IEnumerableAssemblies_DoesNotThrow()
        {
            var container = UtilityMethods.GetContainer();
            List<Assembly> assemblies = new List<Assembly>() { this.GetType().Assembly, typeof(ExternalTypes.IExternalTestInterface).Assembly };

            container.AutoRegister(assemblies);
        }

        [TestMethod]
        public void AutoRegister_IEnumerableAssemblies_TypesFromBothAssembliesResolve()
        {
            var container = UtilityMethods.GetContainer();
            List<Assembly> assemblies = new List<Assembly>() { this.GetType().Assembly, typeof(ExternalTypes.IExternalTestInterface).Assembly };

            container.AutoRegister(assemblies);

            var result1 = container.Resolve<ITestInterface>();
            var result2 = container.Resolve<ExternalTypes.IExternalTestInterface>();

            Assert.IsInstanceOfType(result1, typeof(ITestInterface));
            Assert.IsInstanceOfType(result2, typeof(ExternalTypes.IExternalTestInterface));
        }

#if APPDOMAIN_GETASSEMBLIES
        [TestMethod]
        public void AutoRegister_NoParameters_TypesFromDifferentAssembliesInAppDomainResolve()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister();

            var result1 = container.Resolve<ITestInterface>();
            var result2 = container.Resolve<ExternalTypes.IExternalTestInterface>();

            Assert.IsInstanceOfType(result1, typeof(ITestInterface));
            Assert.IsInstanceOfType(result2, typeof(ExternalTypes.IExternalTestInterface));
        }
#endif

        [TestMethod]
        public void GetChildContainer_NoParameters_ReturnsContainerInstance()
        {
            var container = UtilityMethods.GetContainer();

            var child = container.GetChildContainer();

            Assert.IsInstanceOfType(child, typeof(TinyIoCContainer));
        }

        [TestMethod]
        public void GetChildContainer_NoParameters_ContainerReturnedIsNewContainer()
        {
            var container = UtilityMethods.GetContainer();

            var child = container.GetChildContainer();

            Assert.IsFalse(object.ReferenceEquals(child, container));
        }

        [TestMethod]
        public void ChildContainerResolve_TypeRegisteredWithParent_ResolvesType()
        {
            var container = UtilityMethods.GetContainer();
            var child = container.GetChildContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var result = child.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void ChildContainerCanResolve_TypeRegisteredWithParent_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            var child = container.GetChildContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            var result = child.CanResolve<ITestInterface>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ChildContainerResolve_TypeRegisteredWithChild_ResolvesType()
        {
            var container = UtilityMethods.GetContainer();
            var child = container.GetChildContainer();
            child.Register<ITestInterface, TestClassDefaultCtor>();

            var result = child.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void ChildContainerCanResolve_TypeRegisteredWithChild_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            var child = container.GetChildContainer();
            child.Register<ITestInterface, TestClassDefaultCtor>();

            var result = child.CanResolve<ITestInterface>();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ChildContainerResolve_TypeRegisteredWithParentAndChild_ResolvesChildVersion()
        {
            var container = UtilityMethods.GetContainer();
            var containerInstance = new TestClassDefaultCtor();
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(containerInstance);
            child.Register<ITestInterface>(childInstance);

            var result = child.Resolve<ITestInterface>();

            Assert.AreSame(result, childInstance);
        }

        [TestMethod]
        public void ChildContainerResolve_NamedOnlyRegisteredWithParent_ResolvesFromParent()
        {
            var container = UtilityMethods.GetContainer();
            var containerInstance = new TestClassDefaultCtor();
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(containerInstance, "Testing");
            child.Register<ITestInterface>(childInstance);

            var result = child.Resolve<ITestInterface>("Testing");

            Assert.AreSame(result, containerInstance);
        }

        [TestMethod]
        public void ChildContainerCanResolve_NamedOnlyRegisteredWithParent_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            var containerInstance = new TestClassDefaultCtor();
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(containerInstance, "Testing");
            child.Register<ITestInterface>(childInstance);

            var result = child.CanResolve<ITestInterface>("Testing");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ChildContainerResolve_NamedOnlyRegisteredWithParentUnnamedFallbackOn_ResolvesFromChild()
        {
            var container = UtilityMethods.GetContainer();
            var containerInstance = new TestClassDefaultCtor();
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(containerInstance, "Testing");
            child.Register<ITestInterface>(childInstance);

            var result = child.Resolve<ITestInterface>(new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.AreSame(result, childInstance);
        }

        [TestMethod]
        public void ChildContainerResolve_NamedOnlyRegisteredWithParentChildNoRegistrationUnnamedFallbackOn_ResolvesFromParent()
        {
            var container = UtilityMethods.GetContainer();
            var containerInstance = new TestClassDefaultCtor();
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(containerInstance, "Testing");
            child.Register<ITestInterface>(childInstance);

            var result = child.Resolve<ITestInterface>("Testing", new ResolveOptions() { NamedResolutionFailureAction = NamedResolutionFailureActions.AttemptUnnamedResolution });

            Assert.AreSame(result, containerInstance);
        }

        [TestMethod]
        public void TryResolve_ValidResolve_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>(out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolve_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            ITestInterface output;
            container.TryResolve<ITestInterface>(out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolve_InvalidResolve_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>(out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>(new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>(new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>(new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithName_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>("Testing", out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithName_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            ITestInterface output;
            container.TryResolve<ITestInterface>("Testing", out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithName_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>("Testing", out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithNameAndOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>("Testing", new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithNameAndOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            ITestInterface output;
            container.TryResolve<ITestInterface>("Testing", new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithNameAndOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            ITestInterface output;
            var result = container.TryResolve<ITestInterface>("Testing", new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithParameters_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>(new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithParameters_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>(new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithParameters_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>(new NamedParameterOverloads() { { "intProperty", 2 } }, out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithNameAndParameters_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>("Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithNameAndParameters_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>("Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithNameAndParameters_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>("Testing", new NamedParameterOverloads() { { "intProperty", 2 } }, out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithParametersAndOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>(new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithParametersAndOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>(new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithParametersAndOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>(new NamedParameterOverloads() { { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithNameParametersAndOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>("Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolve_ValidResolveWithNameParametersAndOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>("Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolve_InvalidResolveWithNameParametersAndOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            TestClassWithParameters output;
            var result = container.TryResolve<TestClassWithParameters>("Testing", new NamedParameterOverloads() { { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ResolveAll_MultipleTypesRegistered_ReturnsIEnumerableWithCorrectCount()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface, TestClassDefaultCtor>("Named1");
            container.Register<ITestInterface, TestClassDefaultCtor>("Named2");

            var result = container.ResolveAll<ITestInterface>();

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void ResolveAll_NamedAndUnnamedRegisteredAndPassedTrue_ReturnsIEnumerableWithNamedAndUnnamedRegistrations()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface, TestClassDefaultCtor>("Named1");
            container.Register<ITestInterface, TestClassDefaultCtor>("Named2");

            var result = container.ResolveAll<ITestInterface>(true);

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void ResolveAll_NamedAndUnnamedRegisteredAndPassedFalse_ReturnsIEnumerableWithJustNamedRegistrations()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface, TestClassDefaultCtor>("Named1");
            container.Register<ITestInterface, TestClassDefaultCtor>("Named2");

            var result = container.ResolveAll<ITestInterface>(false);

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void ResolveAll_NoTypesRegistered_ReturnsIEnumerableWithNoItems()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.ResolveAll<ITestInterface>();

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void Resolve_TypeWithIEnumerableOfRegisteredTypeDependency_ResolvesWithIEnumerableOfCorrectCount()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();
            container.Register<ITestInterface, TestClassDefaultCtor>("Named1");
            container.Register<ITestInterface, TestClassDefaultCtor>("Named2");
            container.Register<TestClassEnumerableDependency>();

            var result = container.Resolve<TestClassEnumerableDependency>();

            Assert.AreEqual(2, result.EnumerableCount);
        }

        [TestMethod]
        public void Resolve_TypeWithIEnumerableOfNonRegisteredTypeDependency_ResolvesWithIEnumerablewithNoItems()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassEnumerableDependency>();

            var result = container.Resolve<TestClassEnumerableDependency>();

            Assert.AreEqual(0, result.EnumerableCount);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolve_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolve_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            object output;
            container.TryResolve(typeof(ITestInterface), out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolve_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithName_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            object output;
            var result = container.TryResolve(typeof(ITestInterface), "Testing", out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithName_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            object output;
            container.TryResolve(typeof(ITestInterface), "Testing", out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithName_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), "Testing", out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithNameAndOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            object output;
            var result = container.TryResolve(typeof(ITestInterface), "Testing", new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithNameAndOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("Testing");

            object output;
            container.TryResolve(typeof(ITestInterface), "Testing", new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(ITestInterface));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithNameAndOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(ITestInterface), "Testing", new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithParameters_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithParameters_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithParameters_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), new NamedParameterOverloads() { { "intProperty", 2 } }, out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithNameAndParameters_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), "Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithNameAndParameters_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), "Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithNameAndParameters_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), "Testing", new NamedParameterOverloads() { { "intProperty", 2 } }, out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithParametersAndOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithParametersAndOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithParametersAndOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), new NamedParameterOverloads() { { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithNameParametersAndOptions_ReturnsTrue()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), "Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryResolveNonGeneric_ValidResolveWithNameParametersAndOptions_ReturnsType()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassWithParameters>("Testing");

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), "Testing", new NamedParameterOverloads() { { "stringProperty", "test" }, { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsInstanceOfType(output, typeof(TestClassWithParameters));
        }

        [TestMethod]
        public void TryResolveNonGeneric_InvalidResolveWithNameParametersAndOptions_ReturnsFalse()
        {
            var container = UtilityMethods.GetContainer();

            object output;
            var result = container.TryResolve(typeof(TestClassWithParameters), "Testing", new NamedParameterOverloads() { { "intProperty", 2 } }, new ResolveOptions(), out output);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RegisterNonGeneric_BasicType_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();

            container.Register(typeof(TestClassDefaultCtor));
            var result = container.Resolve<TestClassDefaultCtor>(ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void RegisterNonGeneric_BasicTypeAndName_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();

            container.Register(typeof(TestClassDefaultCtor), "TestClass");
            var result = container.Resolve<TestClassDefaultCtor>("TestClass", ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.IsInstanceOfType(result, typeof(TestClassDefaultCtor));
        }

        [TestMethod]
        public void RegisterNonGeneric_TypeImplementingInterface_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();

            container.Register(typeof(ITestInterface), typeof(TestClassDefaultCtor));
            var result = container.Resolve<ITestInterface>(ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void RegisterNonGeneric_NamedTypeImplementingInterface_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();

            container.Register(typeof(ITestInterface), typeof(TestClassDefaultCtor), "TestClass");
            var result = container.Resolve<ITestInterface>("TestClass", ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void RegisterNonGeneric_BasicTypeAndInstance_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();
            var instance = new TestClassDefaultCtor();

            container.Register(typeof(TestClassDefaultCtor), instance);
            var result = container.Resolve<TestClassDefaultCtor>(ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.AreSame(instance, result);
        }

        [TestMethod]
        public void RegisterNonGeneric_BasicTypeInstanceAndName_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();
            var instance = new TestClassDefaultCtor();

            container.Register(typeof(TestClassDefaultCtor), instance, "TestClass");
            var result = container.Resolve<TestClassDefaultCtor>("TestClass", ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.AreSame(instance, result);
        }

        [TestMethod]
        public void RegisterNonGeneric_TypeImplementingInterfaceAndInstance_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();
            var instance = new TestClassDefaultCtor();

            container.Register(typeof(ITestInterface), typeof(TestClassDefaultCtor), instance);
            var result = container.Resolve<ITestInterface>(ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void RegisterNonGeneric_TypeImplementingInterfaceInstanceAndName_RegistersAndCanResolve()
        {
            var container = UtilityMethods.GetContainer();
            var instance = new TestClassDefaultCtor();

            container.Register(typeof(ITestInterface), typeof(TestClassDefaultCtor), instance, "TestClass");
            var result = container.Resolve<ITestInterface>("TestClass", ResolveOptions.FailUnregisteredAndNameNotFound);

            Assert.IsInstanceOfType(result, typeof(ITestInterface));
        }

        [TestMethod]
        public void RegisterMultiple_Null_Throws()
        {
            var container = UtilityMethods.GetContainer();

            try
            {
                container.RegisterMultiple<ITestInterface>(null);

                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public void RegisterMultiple_ATypeThatDoesntImplementTheRegisterType_Throws()
        {
            var container = UtilityMethods.GetContainer();

            try
            {
                container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(TestClass2) });

                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void RegisterMultiple_ValidTypesButSameTypeMoreThanOnce_Throws()
        {
            var container = UtilityMethods.GetContainer();

            try
            {
                container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(TestClassDefaultCtor) });

                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void RegisterMultiple_TypesThatImplementTheRegisterType_DoesNotThrow()
        {
            var container = UtilityMethods.GetContainer();

            container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(DisposableTestClassWithInterface) });
        }

        [TestMethod]
        public void RegisterMultiple_ValidTypes_ReturnsMultipleRegisterOptions()
        {
            var container = UtilityMethods.GetContainer();

            var result = container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(DisposableTestClassWithInterface) });

            Assert.IsInstanceOfType(result, typeof(TinyIoCContainer.MultiRegisterOptions));
        }

        [TestMethod]
        public void RegisterMultiple_ValidTypes_CorrectCountReturnedByResolveAll()
        {
            var container = UtilityMethods.GetContainer();
            container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(DisposableTestClassWithInterface) });

            var result = container.ResolveAll<ITestInterface>();

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void RegisterMultiple_ValidTypes_InstancesOfCorrectTypesReturnedByResolveAll()
        {
            var container = UtilityMethods.GetContainer();
            container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(DisposableTestClassWithInterface) });

            var result = container.ResolveAll<ITestInterface>();

            Assert.IsNotNull(result.Where(o => o.GetType() == typeof(TestClassDefaultCtor)).FirstOrDefault());
            Assert.IsNotNull(result.Where(o => o.GetType() == typeof(DisposableTestClassWithInterface)).FirstOrDefault());
        }

        [TestMethod]
        public void RegisterMultiple_ValidTypesRegisteredAsSingleton_AlwaysReturnsSameInstance()
        {
            var container = UtilityMethods.GetContainer();
            container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(DisposableTestClassWithInterface) }).AsSingleton();

            var result1 = container.ResolveAll<ITestInterface>();
            var result2 = container.ResolveAll<ITestInterface>();
            var result1Class1Instance = result1.Where(o => o.GetType() == typeof(TestClassDefaultCtor)).FirstOrDefault();
            var result2Class1Instance = result2.Where(o => o.GetType() == typeof(TestClassDefaultCtor)).FirstOrDefault();
            var result1Class2Instance = result1.Where(o => o.GetType() == typeof(DisposableTestClassWithInterface)).FirstOrDefault();
            var result2Class2Instance = result2.Where(o => o.GetType() == typeof(DisposableTestClassWithInterface)).FirstOrDefault();

            Assert.AreSame(result1Class1Instance, result2Class1Instance);
            Assert.AreSame(result1Class2Instance, result2Class2Instance);
        }

        [TestMethod]
        public void RegisterMultiple_ValidTypesRegisteredAsMultiInstance_AlwaysReturnsNewInstance()
        {
            var container = UtilityMethods.GetContainer();
            container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassDefaultCtor), typeof(DisposableTestClassWithInterface) }).AsMultiInstance();

            var result1 = container.ResolveAll<ITestInterface>();
            var result2 = container.ResolveAll<ITestInterface>();
            var result1Class1Instance = result1.Where(o => o.GetType() == typeof(TestClassDefaultCtor)).FirstOrDefault();
            var result2Class1Instance = result2.Where(o => o.GetType() == typeof(TestClassDefaultCtor)).FirstOrDefault();
            var result1Class2Instance = result1.Where(o => o.GetType() == typeof(DisposableTestClassWithInterface)).FirstOrDefault();
            var result2Class2Instance = result2.Where(o => o.GetType() == typeof(DisposableTestClassWithInterface)).FirstOrDefault();

            Assert.IsFalse(object.ReferenceEquals(result1Class1Instance, result2Class1Instance));
            Assert.IsFalse(object.ReferenceEquals(result1Class2Instance, result2Class2Instance));
        }

        [TestMethod]
        public void Resolve_MultiInstanceTypeInParentContainerButDependencyInChildContainer_GetsDependencyFromChild()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClassWithInterfaceDependency>().AsMultiInstance();
            var parentInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(parentInstance);
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            child.Register<ITestInterface>(childInstance);
            container.Resolve<ITestInterface2>();

            var result = child.Resolve<ITestInterface2>() as TestClassWithInterfaceDependency;

            Assert.IsTrue(object.ReferenceEquals(result.Dependency, childInstance));
        }

        [TestMethod]
        public void Resolve_AlreadyResolvedSingletonTypeInParentContainerButDependencyInChildContainer_GetsDependencyFromParent()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClassWithInterfaceDependency>().AsSingleton();
            var parentInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(parentInstance);
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            child.Register<ITestInterface>(childInstance);
            container.Resolve<ITestInterface2>();

            var result = child.Resolve<ITestInterface2>() as TestClassWithInterfaceDependency;

            Assert.IsTrue(object.ReferenceEquals(result.Dependency, parentInstance));
        }

        [TestMethod]
        public void Resolve_NotAlreadyResolvedSingletonTypeInParentContainerButDependencyInChildContainer_GetsDependencyFromParent()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface2, TestClassWithInterfaceDependency>().AsSingleton();
            var parentInstance = new TestClassDefaultCtor();
            container.Register<ITestInterface>(parentInstance);
            var child = container.GetChildContainer();
            var childInstance = new TestClassDefaultCtor();
            child.Register<ITestInterface>(childInstance);

            var result = child.Resolve<ITestInterface2>() as TestClassWithInterfaceDependency;

            Assert.IsTrue(object.ReferenceEquals(result.Dependency, parentInstance));
        }

        [TestMethod]
        public void Resolve_ContainerHierarchy_ResolvesCorrectlyUsingHierarchy()
        {
            var rootContainer = UtilityMethods.GetContainer();
            var firstChildContainer = rootContainer.GetChildContainer();
            var secondChildContainer = firstChildContainer.GetChildContainer();
            var rootInstance = new TestClassDefaultCtor();
            var firstChildInstance = new TestClassDefaultCtor();
            var secondChildInstance = new TestClassDefaultCtor();
            rootContainer.Register<ITestInterface2, TestClassWithInterfaceDependency>().AsMultiInstance();
            rootContainer.Register<ITestInterface>(rootInstance);
            firstChildContainer.Register<ITestInterface>(firstChildInstance);
            secondChildContainer.Register<ITestInterface>(secondChildInstance);
            rootContainer.Resolve<ITestInterface2>();

            var result = secondChildContainer.Resolve<ITestInterface2>() as TestClassWithInterfaceDependency;

            Assert.IsTrue(object.ReferenceEquals(result.Dependency, secondChildInstance));
        }

        [TestMethod]
        public void ResolveAll_ChildContainerNoRegistrationsParentContainerHasRegistrations_ReturnsAllParentRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");

            var result = childContainer.ResolveAll<ITestInterface>();

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void ResolveAll_ChildContainerHasRegistrationsParentContainerHasRegistrations_ReturnsAllRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "4");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "5");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "6");

            var result = childContainer.ResolveAll<ITestInterface>();

            Assert.AreEqual(6, result.Count());
        }

        [TestMethod]
        public void ResolveAll_ChildContainerRegistrationsParentContainerNoRegistrations_ReturnsAllChildRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");

            var result = childContainer.ResolveAll<ITestInterface>();

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void ResolveAll_ChildContainerRegistrationsOverrideParentContainerRegistrations_ReturnsChildRegistrationsWithoutDuplicates()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");

            var result = childContainer.ResolveAll<ITestInterface>();

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void ResolveAll_ChildContainerRegistrationOverridesParentContainerRegistration_ReturnsChildRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            var parentInstance = new TestClassDefaultCtor();
            var childInstance = new TestClassDefaultCtor();
            parentContainer.Register<ITestInterface>(parentInstance, "1");
            childContainer.Register<ITestInterface>(childInstance, "1");

            var result = childContainer.ResolveAll<ITestInterface>();

            Assert.AreEqual(1, result.Count());
            Assert.AreSame(childInstance, result.Single());
        }

        [TestMethod]
        public void ResolveAll_ParentContainerMultiInstanceRegistrationWithDependencyInChildContainer_ReturnsRegistrationWithChildContainerDependencyInstance()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            var parentInstance = new TestClassDefaultCtor();
            var childInstance = new TestClassDefaultCtor();
            parentContainer.Register<ITestInterface2, TestClassWithInterfaceDependency>("1").AsMultiInstance();
            parentContainer.Register<ITestInterface>(parentInstance);
            childContainer.Register<ITestInterface2, TestClassWithInterfaceDependency>("2").AsMultiInstance();
            childContainer.Register<ITestInterface>(childInstance);

            var result = childContainer.ResolveAll<ITestInterface2>(false).ToArray();
            var item1 = result[0] as TestClassWithInterfaceDependency;
            var item2 = result[1] as TestClassWithInterfaceDependency;

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);
            Assert.IsFalse(object.ReferenceEquals(item1, item2), "items are same instance");
            Assert.IsTrue(object.ReferenceEquals(item1.Dependency, childInstance), "item1 has wrong dependency");
            Assert.IsTrue(object.ReferenceEquals(item2.Dependency, childInstance), "item2 has wrong dependency");
        }

        [TestMethod]
        public void Resolve_IEnumerableDependencyClassInChildContainerChildContainerNoRegistrationsParentContainerHasRegistrations_ReturnsAllParentRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");
            childContainer.Register<TestClassEnumerableDependency>();

            var result = childContainer.Resolve<TestClassEnumerableDependency>();

            Assert.AreEqual(3, result.EnumerableCount);
        }

        [TestMethod]
        public void Resolve_IEnumerableDependencyClassInChildContainerChildContainerHasRegistrationsParentContainerHasRegistrations_ReturnsAllRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            parentContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "4");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "5");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "6");
            childContainer.Register<TestClassEnumerableDependency>();

            var result = childContainer.Resolve<TestClassEnumerableDependency>();

            Assert.AreEqual(6, result.EnumerableCount);
        }

        [TestMethod]
        public void Resolve_IEnumerableDependencyClassInChildContainerChildContainerRegistrationsParentContainerNoRegistrations_ReturnsAllChildRegistrations()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "1");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "2");
            childContainer.Register<ITestInterface>(new TestClassDefaultCtor(), "3");
            childContainer.Register<TestClassEnumerableDependency>();

            var result = childContainer.Resolve<TestClassEnumerableDependency>();

            Assert.AreEqual(3, result.EnumerableCount);
        }

        [TestMethod]
        public void Resolve_IEnumerableDependencyClassInChildContainerParentContainerMultiInstanceRegistrationWithDependencyInChildContainer_ReturnsRegistrationWithChildContainerDependencyInstance()
        {
            var parentContainer = UtilityMethods.GetContainer();
            var childContainer = parentContainer.GetChildContainer();
            var parentInstance = new TestClassDefaultCtor();
            var childInstance = new TestClassDefaultCtor();
            parentContainer.Register<ITestInterface2, TestClassWithInterfaceDependency>("1").AsMultiInstance();
            parentContainer.Register<ITestInterface>(parentInstance);
            childContainer.Register<ITestInterface2, TestClassWithInterfaceDependency>("2").AsMultiInstance();
            childContainer.Register<ITestInterface>(childInstance);
            childContainer.Register<TestClassEnumerableDependency2>();

            var result = childContainer.Resolve<TestClassEnumerableDependency2>().Enumerable.ToArray();
            var item1 = result[0] as TestClassWithInterfaceDependency;
            var item2 = result[1] as TestClassWithInterfaceDependency;

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);
            Assert.IsFalse(object.ReferenceEquals(item1, item2), "items are same instance");
            Assert.IsTrue(object.ReferenceEquals(item1.Dependency, childInstance), "item1 has wrong dependency");
            Assert.IsTrue(object.ReferenceEquals(item2.Dependency, childInstance), "item2 has wrong dependency");
        }

        [TestMethod]
        public void ToCustomLifetimeProvider_NullInstance_Throws()
        {
            var container = UtilityMethods.GetContainer();

            try
            {
                TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(null, null, "");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException)
            {

            }
        }
        
        [TestMethod]
        public void ToCustomLifetimeProvider_NullProvider_Throws()
        {
            var container = UtilityMethods.GetContainer();
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();

            try
            {
                TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, null, "");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException)
            {
                
            }
        }

        [TestMethod]
        public void ToCustomLifetimeProvider_NullErrorString_Throws()
        {
            var container = UtilityMethods.GetContainer();
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();

            try
            {
                TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, new Fakes.FakeLifetimeProvider(), null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {

            }
        }

        [TestMethod]
        public void ToCustomLifetimeProvider_EmptyErrorString_Throws()
        {
            var container = UtilityMethods.GetContainer();
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();

            try
            {
                TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, new Fakes.FakeLifetimeProvider(), "");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {

            }
        }

#if MOQ
        [TestMethod]
        public void CustomLifetimeProvider_WhenResolved_CallsGetObjectOnLifetimeProvider()
        {
            var container = UtilityMethods.GetContainer();
            var providerMock = new Mock<TinyIoCContainer.ITinyIoCObjectLifetimeProvider>();
            providerMock.Setup(p => p.GetObject()).Returns(new TestClassDefaultCtor());
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();
            TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, providerMock.Object, "Mock");

            container.Resolve<ITestInterface>();

            providerMock.Verify(p => p.GetObject(), Times.Once(), "not called");
        }
#endif

#if MOQ
        [TestMethod]
        public void CustomLifetimeProvider_GetObjectReturnsNull_CallsSetObjectOnProvider()
        {
            var container = UtilityMethods.GetContainer();
            var providerMock = new Mock<TinyIoCContainer.ITinyIoCObjectLifetimeProvider>();
            providerMock.Setup(p => p.GetObject()).Returns(null);
            providerMock.Setup(p => p.SetObject(It.IsAny<object>())).Verifiable();
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();
            TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, providerMock.Object, "Mock");

            container.Resolve<ITestInterface>();

            providerMock.Verify(p => p.SetObject(It.IsAny<object>()), Times.Once(), "not called");
        }
#endif

#if MOQ
        [TestMethod]
        public void CustomLifetimeProvider_SwitchingToAnotherFactory_CallsReleaseObjectOnProvider()
        {
            var container = UtilityMethods.GetContainer();
            var providerMock = new Mock<TinyIoCContainer.ITinyIoCObjectLifetimeProvider>();
            providerMock.Setup(p => p.ReleaseObject()).Verifiable();
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();
            registration = TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, providerMock.Object, "Mock");

            registration.AsSingleton();

            providerMock.Verify(p => p.ReleaseObject(), Times.AtLeastOnce(), "not called");
        }
#endif

#if MOQ
        [TestMethod]
        public void CustomLifetimeProvider_ContainerDisposed_CallsReleaseObjectOnProvider()
        {
            var container = UtilityMethods.GetContainer();
            var providerMock = new Mock<TinyIoCContainer.ITinyIoCObjectLifetimeProvider>();
            providerMock.Setup(p => p.ReleaseObject()).Verifiable();
            var registration = container.Register<ITestInterface, TestClassDefaultCtor>();
            TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registration, providerMock.Object, "Mock");

            container.Dispose();

            providerMock.Verify(p => p.ReleaseObject(), Times.AtLeastOnce(), "not called");
        }
#endif

        [TestMethod]
        public void AutoRegister_TypeExcludedViaPredicate_FailsToResolveType()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { this.GetType().Assembly }, t => t != typeof(ITestInterface));

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<ITestInterface>());

            //Assert.IsInstanceOfType(result, typeof(TinyIoCResolutionException));
        }

#if RESOLVE_OPEN_GENERICS
        [TestMethod]
        public void Register_OpenGeneric_DoesNotThrow()
        {
            var container = UtilityMethods.GetContainer();

            container.Register(typeof(IThing<>), typeof(DefaultThing<>));
        }
#endif

#if RESOLVE_OPEN_GENERICS
        [TestMethod]
        public void Resolve_RegisteredOpenGeneric_ReturnsInstance()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(IThing<>), typeof(DefaultThing<>));

            var result = container.Resolve<IThing<object>>();

            Assert.IsInstanceOfType(result, typeof(DefaultThing<object>));
        }
#endif


#if RESOLVE_OPEN_GENERICS
        [TestMethod]
        public void Resolve_RegisteredOpenGenericInParent_CanBeResolvedByChild()
        {
            var container = UtilityMethods.GetContainer();
            container.Register(typeof(IThing<>), typeof(DefaultThing<>));

            var child = container.GetChildContainer();

            var result = child.Resolve<IThing<object>>();
             
            Assert.IsInstanceOfType(result, typeof(DefaultThing<object>));
        }
#endif

        #region Unregister

        private readonly ResolveOptions options = ResolveOptions.FailUnregisteredAndNameNotFound;

        #region Unregister With Implementation

        [TestMethod]
        public void Unregister_RegisteredImplementation_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            bool unregistered = container.Unregister(typeof(TestClassDefaultCtor));
            bool resolved = container.CanResolve<TestClassDefaultCtor>(options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_NotRegisteredImplementation_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();

            bool unregistered = container.Unregister(typeof(TestClassDefaultCtor));
            
            Assert.IsFalse(unregistered);
        }

        [TestMethod]
        public void Unregister_RegisteredNamedImplementation_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister(typeof(TestClassDefaultCtor), "TestName");
            bool resolved = container.CanResolve<TestClassDefaultCtor>("TestName", options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_NotRegisteredNamedImplementation_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister(typeof(TestClassDefaultCtor), "UnregisteredName");
            bool resolved = container.CanResolve<TestClassDefaultCtor>("TestName", options);

            Assert.IsFalse(unregistered);
            Assert.IsTrue(resolved);
        }

        #endregion

        #region Unregister With Interface

        [TestMethod]
        public void Unregister_RegisteredInterface_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            bool unregistered = container.Unregister(typeof(ITestInterface));
            bool resolved = container.CanResolve<ITestInterface>(options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_NotRegisteredInterface_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();

            bool unregistered = container.Unregister(typeof(ITestInterface));

            Assert.IsFalse(unregistered);
        }

        [TestMethod]
        public void Unregister_RegisteredNamedInterface_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister(typeof(ITestInterface), "TestName");
            bool resolved = container.CanResolve<ITestInterface>("TestName", options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_NotRegisteredNamedInterface_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister(typeof(ITestInterface), "UnregisteredName");
            bool resolved = container.CanResolve<ITestInterface>("TestName", options);

            Assert.IsFalse(unregistered);
            Assert.IsTrue(resolved);
        }

        #endregion

        #region Unregister<T> With Implementation

        [TestMethod]
        public void Unregister_T_RegisteredImplementation_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>();

            bool unregistered = container.Unregister<TestClassDefaultCtor>();
            bool resolved = container.CanResolve<TestClassDefaultCtor>(options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_T_NotRegisteredImplementation_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();

            bool unregistered = container.Unregister<TestClassDefaultCtor>();

            Assert.IsFalse(unregistered);
        }

        [TestMethod]
        public void Unregister_T_RegisteredNamedImplementation_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister<TestClassDefaultCtor>("TestName");
            bool resolved = container.CanResolve<TestClassDefaultCtor>("TestName", options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_T_NotRegisteredNamedImplementation_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister<TestClassDefaultCtor>("UnregisteredName");
            bool resolved = container.CanResolve<TestClassDefaultCtor>("TestName", options);

            Assert.IsFalse(unregistered);
            Assert.IsTrue(resolved);
        }

        #endregion

        #region Unregister<T> With Interface

        [TestMethod]
        public void Unregister_T_RegisteredInterface_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>();

            bool unregistered = container.Unregister<ITestInterface>();
            bool resolved = container.CanResolve<ITestInterface>(options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_T_NotRegisteredInterface_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();

            bool unregistered = container.Unregister<ITestInterface>();

            Assert.IsFalse(unregistered);
        }

        [TestMethod]
        public void Unregister_T_RegisteredNamedInterface_CanUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister<ITestInterface>("TestName");
            bool resolved = container.CanResolve<ITestInterface>("TestName", options);

            Assert.IsTrue(unregistered);
            Assert.IsFalse(resolved);
        }

        [TestMethod]
        public void Unregister_T_NotRegisteredNamedInterface_CannotUnregister()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<ITestInterface, TestClassDefaultCtor>("TestName");

            bool unregistered = container.Unregister<ITestInterface>("UnregisteredName");
            bool resolved = container.CanResolve<ITestInterface>("TestName", options);

            Assert.IsFalse(unregistered);
            Assert.IsTrue(resolved);
        }

        #endregion

        #endregion
    }
}
