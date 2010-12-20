using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace TinyIoC.Tests.TypeExtensions
{
    public interface ITestInterface
    {
    }

    public class ClassImplementingITestInterface : ITestInterface
    {
    }

    public class ClassNotImplementingITestInterface
    {
    }

    [TestClass]
    public class TypeExtensionsTests
    {
        [TestMethod]
        public void GetGenericMethod_RegisterOneGenericParameterNoParameters_ReturnsCorrectMethod()
        {
            var firstGenericParameter = typeof(ClassNotImplementingITestInterface);

            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance, 
                "Register", 
                new Type[] {firstGenericParameter}, 
                new Type[] { }
                );

            Assert.IsInstanceOfType(method, typeof(MethodInfo));
            Assert.IsTrue(method.IsGenericMethod);
            Assert.AreEqual(0, method.GetParameters().Length);
            Assert.AreEqual(1, method.GetGenericArguments().Length);
            Assert.AreEqual(firstGenericParameter, method.GetGenericArguments()[0]);
        }

        [TestMethod]
        public void GetGenericMethod_RegisterTwoAcceptableGenericParameterNoParameters_ReturnsCorrectMethod()
        {
            var firstGenericParameter = typeof(ITestInterface);
            var secondGenericParameter = typeof(ClassImplementingITestInterface);

            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { }
                );

            Assert.IsInstanceOfType(method, typeof(MethodInfo));
            Assert.IsTrue(method.IsGenericMethod);
            Assert.AreEqual(0, method.GetParameters().Length);
            Assert.AreEqual(2, method.GetGenericArguments().Length);
            Assert.AreEqual(firstGenericParameter, method.GetGenericArguments()[0]);
            Assert.AreEqual(secondGenericParameter, method.GetGenericArguments()[1]);
        }

        [TestMethod]
        public void GetGenericMethod_RegisterTwoUnacceptableGenericParameterNoParameters_Throws()
        {
            try
            {
                var method = typeof(TinyIoCContainer).GetGenericMethod(
                    BindingFlags.Public | BindingFlags.Instance,
                    "Register",
                    new Type[] { typeof(ITestInterface), typeof(ClassNotImplementingITestInterface) },
                    new Type[] { }
                    );

                Assert.Fail();
            }
            catch (System.ArgumentException)
            {
            }
        }

        [TestMethod]
        public void GetGenericMethod_RegisterTwoAcceptableGenericParameterMethodParameters_ReturnsCorrectMethod()
        {
            var firstGenericParameter = typeof(ITestInterface);
            var secondGenericParameter = typeof(ClassImplementingITestInterface);
            var firstParameter = typeof(string);

            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { firstParameter }
                );

            Assert.IsInstanceOfType(method, typeof(MethodInfo));
            Assert.IsTrue(method.IsGenericMethod);
            Assert.AreEqual(1, method.GetParameters().Length);
            Assert.AreEqual(firstParameter, method.GetParameters()[0].ParameterType);
            Assert.AreEqual(2, method.GetGenericArguments().Length);
            Assert.AreEqual(firstGenericParameter, method.GetGenericArguments()[0]);
            Assert.AreEqual(secondGenericParameter, method.GetGenericArguments()[1]);
        }

        [TestMethod]
        public void GetGenericMethod_RegisterWithGenericTypeAsAMethodParameter_ReturnsCorrectMethod()
        {
            var firstGenericParameter = typeof(ITestInterface);
            var secondGenericParameter = typeof(ClassImplementingITestInterface);
            var firstParameter = typeof(ClassImplementingITestInterface);

            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { firstParameter }
                );

            Assert.IsInstanceOfType(method, typeof(MethodInfo));
            Assert.IsTrue(method.IsGenericMethod);
            Assert.AreEqual(1, method.GetParameters().Length);
            Assert.AreEqual(firstParameter, method.GetParameters()[0].ParameterType);
            Assert.AreEqual(2, method.GetGenericArguments().Length);
            Assert.AreEqual(firstGenericParameter, method.GetGenericArguments()[0]);
            Assert.AreEqual(secondGenericParameter, method.GetGenericArguments()[1]);
        }
    }
}
