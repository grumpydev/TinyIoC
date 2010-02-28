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
            container.AutoRegister(this.GetType().Assembly);

            var result = container.Resolve<NestedInterfaceDependencies.RootClass>();

            Assert.IsInstanceOfType(result, typeof(NestedInterfaceDependencies.RootClass));
        }

    }
}
