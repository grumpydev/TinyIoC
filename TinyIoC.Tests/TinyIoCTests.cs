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
        internal interface ITestInterface
        {
        }

        internal class TestClass : ITestInterface
        {
            public TestClass()
            {
                
            }
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
            TinyIoC.Register<TestClass>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Register_InterfaceAndImplementation_CanRegister()
        {
            TinyIoC.Register<ITestInterface, TestClass>();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_RegisteredType_ReturnsInstanceOfCorrectType()
        {
            TinyIoC.Register<ITestInterface, TestClass>();

            var output = TinyIoC.Resolve<ITestInterface>();

            Assert.IsInstanceOfType(output, typeof(TestClass));
        }

    }
}
