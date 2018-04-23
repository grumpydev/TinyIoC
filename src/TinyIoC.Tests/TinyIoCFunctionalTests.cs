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
using System.Reflection;
using TinyIoC.Tests.TestData;
using TinyIoC.Tests.TestData.BasicClasses;
using NestedInterfaceDependencies = TinyIoC.Tests.TestData.NestedInterfaceDependencies;
using NestedClassDependencies = TinyIoC.Tests.TestData.NestedClassDependencies;

#if !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace TinyIoC.Tests
{
    using NestedInterfaceDependencies;

    using TinyIoC.Tests.PlatformTestSuite;

    [TestClass]
    public class TinyIoCFunctionalTests
    {
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
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void NestedInterfaceDependencies_MissingIService3Registration_ThrowsExceptionWithDefaultSettings()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedInterfaceDependencies.IService1, NestedInterfaceDependencies.Service1>();
            container.Register<NestedInterfaceDependencies.IService2, NestedInterfaceDependencies.Service2>();
            container.Register<NestedInterfaceDependencies.RootClass>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<NestedInterfaceDependencies.RootClass>());

            //Assert.IsInstanceOfType(result, typeof(NestedInterfaceDependencies.RootClass));
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
        public void NestedClassDependencies_UsingConstructorFromAnotherType_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            var registerOptions = container.Register<NestedClassDependencies.RootClass>();

            AssertHelper.ThrowsException<TinyIoCConstructorResolutionException>
                (() => registerOptions.UsingConstructor(() => new RootClass(null, null)));
        }

        [TestMethod]
        public void NestedClassDependencies_MissingService3Registration_ResolvesRootResolutionOn()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedClassDependencies.Service1>();
            container.Register<NestedClassDependencies.Service2>();
            container.Register<NestedClassDependencies.RootClass>();

            var result = container.Resolve<NestedClassDependencies.RootClass>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.AttemptResolve });

            Assert.IsInstanceOfType(result, typeof(NestedClassDependencies.RootClass));
        }

        [TestMethod]
        //[ExpectedException(typeof(TinyIoCResolutionException))]
        public void NestedClassDependencies_MissingService3RegistrationAndUnRegisteredResolutionOff_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedClassDependencies.Service1>();
            container.Register<NestedClassDependencies.Service2>();
            container.Register<NestedClassDependencies.RootClass>();

            AssertHelper.ThrowsException<TinyIoCResolutionException>(() => container.Resolve<NestedClassDependencies.RootClass>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail }));

            //Assert.IsInstanceOfType(result, typeof(NestedClassDependencies.RootClass));
        }

        [TestMethod]
        public void NestedInterfaceDependencies_JustAutoRegisterCalled_ResolvesRoot()
        {
            var container = UtilityMethods.GetContainer();
            container.AutoRegister(new[] { this.GetType().Assembly });

            var result = container.Resolve<NestedInterfaceDependencies.RootClass>();

            Assert.IsInstanceOfType(result, typeof(NestedInterfaceDependencies.RootClass));
        }

        [TestMethod]
        public void Dependency_Hierarchy_With_Named_Factories_Resolves_All_Correctly()
        {
            var container = UtilityMethods.GetContainer();
            var mainView = new MainView();
            container.Register<IViewManager>(mainView);
            container.Register<IView, MainView>(mainView, "MainView");
            container.Register<IView, SplashView>("SplashView").UsingConstructor(() => new SplashView());
            container.Resolve<IView>("MainView");
            container.Register<IStateManager, StateManager>();
            var stateManager = container.Resolve<IStateManager>();

            stateManager.Init();

            Assert.IsInstanceOfType(mainView.LoadedView, typeof(SplashView));
        }

        [TestMethod]
        public void Dependency_Hierarchy_Resolves_IEnumerable_Correctly()
        {
            var container = UtilityMethods.GetContainer();
            var mainView = new MainView();
            container.Register<IView, MainView>(mainView, "MainView");
            container.Register<IView, SplashView>("SplashView").UsingConstructor(() => new SplashView());
            var viewCollection = container.Resolve<ViewCollection>();
            Assert.AreEqual(viewCollection.Views.Count(), 2);
        }

        [TestMethod]
        public void When_Unable_To_Resolve_Nested_Dependency_Should_Include_That_Type_In_The_Exception()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedInterfaceDependencies.IService1, NestedInterfaceDependencies.Service1>();
            container.Register<NestedInterfaceDependencies.IService2, NestedInterfaceDependencies.Service2>();
            container.Register<NestedInterfaceDependencies.IRoot, NestedInterfaceDependencies.RootClass>();

            TinyIoCResolutionException e = null;
            try
            {
                container.Resolve<NestedInterfaceDependencies.IRoot>();
            }
            catch (TinyIoCResolutionException ex)
            {
                e = ex;
            }

            Assert.IsNotNull(e);
            Assert.IsTrue(e.ToString().Contains("NestedInterfaceDependencies.IService3"));
        }

        [TestMethod]
        public void When_Unable_To_Resolve_Non_Nested_Dependency_Should_Include_That_Type_In_The_Exception()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedInterfaceDependencies.IService2, NestedInterfaceDependencies.Service2>();
            container.Register<NestedInterfaceDependencies.IService3, NestedInterfaceDependencies.Service3>();
            container.Register<NestedInterfaceDependencies.IRoot, NestedInterfaceDependencies.RootClass>();

            TinyIoCResolutionException e = null;
            try
            {
                container.Resolve<NestedInterfaceDependencies.IRoot>();
            }
            catch (TinyIoCResolutionException ex)
            {
                e = ex;
            }

            Assert.IsNotNull(e);
            Assert.IsTrue(e.ToString().Contains("NestedInterfaceDependencies.IService1"));
        }

        [TestMethod]
        public void Run_Platform_Tests()
        {
            var logger = new StringLogger();
            var platformTests = new PlatformTestSuite.PlatformTests(logger);

            int failed;
            int run;
            int passed;
            platformTests.RunTests(out run, out passed, out failed);

            Assert.AreEqual(0, failed, logger.Log);
        }

        [TestMethod]
        public void Resolve_InterfacesAcrossInChildContainer_Resolves()
        {
            var container = UtilityMethods.GetContainer();

            container.Register<IService2, Service2>().AsMultiInstance();

            container.Register<IService4, Service4>().AsMultiInstance();

            container.Register<IService5, Service5>().AsMultiInstance();

            var child = container.GetChildContainer();

            var nestedService = new Service3();
            child.Register<IService3>(nestedService);

            var service5 = child.Resolve<IService5>();

            Assert.IsNotNull(service5.Service4);

            Assert.AreSame(nestedService, service5.Service4.Service2.Service3);
        }
    }
}
