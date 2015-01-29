using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

#if !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace TinyIoC.Tests.TypeExtensions
{
    public interface ITestInterface
    {
    }

    public class ClassImplementingITestInterface : ITestInterface
    {
    }

    public class AnotherClassImplementingITestInterface : ITestInterface
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

#if NETFX_CORE
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                "Register",
                new Type[] { firstGenericParameter },
                new Type[] { }
                );
#else
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter },
                new Type[] { }
                );
#endif

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

#if NETFX_CORE
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { }
                );
#else
              var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { }
                );
#endif

            Assert.IsInstanceOfType(method, typeof(MethodInfo));
            Assert.IsTrue(method.IsGenericMethod);
            Assert.AreEqual(0, method.GetParameters().Length);
            Assert.AreEqual(2, method.GetGenericArguments().Length);
            Assert.AreEqual(firstGenericParameter, method.GetGenericArguments()[0]);
            Assert.AreEqual(secondGenericParameter, method.GetGenericArguments()[1]);
        }

        [TestMethod]
        public void GetGenericMethod_TwiceWithDifferentGenericParamters_ReturnsCorrectMethods()
        {
            var methodOneFirstGenericParameter = typeof(ITestInterface);
            var methodOneSecondGenericParameter = typeof(ClassImplementingITestInterface);
            var methodTwoFirstGenericParameter = typeof(ITestInterface);
            var methodTwoSecondGenericParameter = typeof(AnotherClassImplementingITestInterface);

#if NETFX_CORE
            var methodOne = typeof(TinyIoCContainer).GetGenericMethod(
                "Register",
                new Type[] { methodOneFirstGenericParameter, methodOneSecondGenericParameter },
                new Type[] { });
            var methodTwo = typeof(TinyIoCContainer).GetGenericMethod(
                "Register",
                new Type[] { methodTwoFirstGenericParameter, methodTwoSecondGenericParameter },
                new Type[] { });
#else
            var methodOne = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { methodOneFirstGenericParameter, methodOneSecondGenericParameter },
                new Type[] { });
            var methodTwo = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { methodTwoFirstGenericParameter, methodTwoSecondGenericParameter },
                new Type[] { });
#endif

            Assert.IsInstanceOfType(methodOne, typeof(MethodInfo));
            Assert.IsTrue(methodOne.IsGenericMethod);
            Assert.AreEqual(0, methodOne.GetParameters().Length);
            Assert.AreEqual(2, methodOne.GetGenericArguments().Length);
            Assert.AreEqual(methodOneFirstGenericParameter, methodOne.GetGenericArguments()[0]);
            Assert.AreEqual(methodOneSecondGenericParameter, methodOne.GetGenericArguments()[1]);
            Assert.IsInstanceOfType(methodTwo, typeof(MethodInfo));
            Assert.IsTrue(methodTwo.IsGenericMethod);
            Assert.AreEqual(0, methodTwo.GetParameters().Length);
            Assert.AreEqual(2, methodTwo.GetGenericArguments().Length);
            Assert.AreEqual(methodTwoFirstGenericParameter, methodTwo.GetGenericArguments()[0]);
            Assert.AreEqual(methodTwoSecondGenericParameter, methodTwo.GetGenericArguments()[1]);
        }

        [TestMethod]
        public void GetGenericMethod_RegisterTwoUnacceptableGenericParameterNoParameters_Throws()
        {
            try
            {
#if NETFX_CORE
                var method = typeof(TinyIoCContainer).GetGenericMethod(
                    "Register",
                    new Type[] { typeof(ITestInterface), typeof(ClassNotImplementingITestInterface) },
                    new Type[] { }
                    );
#else
                var method = typeof(TinyIoCContainer).GetGenericMethod(
                    BindingFlags.Public | BindingFlags.Instance,
                    "Register",
                    new Type[] { typeof(ITestInterface), typeof(ClassNotImplementingITestInterface) },
                    new Type[] { }
                    );
#endif

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

#if NETFX_CORE
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { firstParameter }
                );
#else
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { firstParameter }
                );
#endif

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

#if NETFX_CORE
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { firstParameter }
                );
#else
            var method = typeof(TinyIoCContainer).GetGenericMethod(
                BindingFlags.Public | BindingFlags.Instance,
                "Register",
                new Type[] { firstGenericParameter, secondGenericParameter },
                new Type[] { firstParameter }
                );
#endif

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
