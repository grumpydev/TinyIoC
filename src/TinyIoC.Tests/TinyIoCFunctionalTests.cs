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
using TinyIoC.Tests.TestData;
using TinyIoC.Tests.TestData.BasicClasses;
using NestedInterfaceDependencies = TinyIoC.Tests.TestData.NestedInterfaceDependencies;
using NestedClassDependencies = TinyIoC.Tests.TestData.NestedClassDependencies;

namespace TinyIoC.Tests
{
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
        [ExpectedException(typeof(TinyIoCResolutionException))]
        public void NestedClassDependencies_MissingService3RegistrationAndUnRegisteredResolutionOff_ThrowsException()
        {
            var container = UtilityMethods.GetContainer();
            container.Register<NestedClassDependencies.Service1>();
            container.Register<NestedClassDependencies.Service2>();
            container.Register<NestedClassDependencies.RootClass>();

            var result = container.Resolve<NestedClassDependencies.RootClass>(new ResolveOptions() { UnregisteredResolutionAction = UnregisteredResolutionActions.Fail });

            Assert.IsInstanceOfType(result, typeof(NestedClassDependencies.RootClass));
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
    }
}
