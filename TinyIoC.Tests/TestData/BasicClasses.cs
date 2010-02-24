using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC.Tests.TestData
{
    namespace BasicClasses
    {
        internal interface ITestInterface
        {
        }

        internal class TestClassDefaultCtor : ITestInterface
        {
            public string Prop1 { get; set; }

            public TestClassDefaultCtor()
            {

            }

            public static ITestInterface CreateNew(TinyIoC container)
            {
                return new TestClassDefaultCtor() { Prop1 = "Testing" };
            }
        }

        internal interface ITestInterface2
        {
        }

        internal class TestClassWithContainerDependency
        {
            public TinyIoC _Container { get; private set; }

            public TestClassWithContainerDependency(TinyIoC container)
            {
                if (container == null)
                    throw new ArgumentNullException("container");

                _Container = container;
            }
        }

        internal class TestClassWithInterfaceDependency : ITestInterface2
        {
            public ITestInterface Dependency { get; set; }

            public int Param1 { get; private set; }

            public string Param2 { get; private set; }

            public TestClassWithInterfaceDependency(ITestInterface dependency)
            {
                if (dependency == null)
                    throw new ArgumentNullException("dependency");

                Dependency = dependency;
            }

            public TestClassWithInterfaceDependency(ITestInterface dependency, int param1, string param2)
            {
                Dependency = dependency;
                Param1 = param1;
                Param2 = param2;
            }
        }

        internal class TestClassWithDependency
        {
            TestClassDefaultCtor Dependency { get; set; }

            public int Param1 { get; private set; }

            public string Param2 { get; private set; }

            public TestClassWithDependency(TestClassDefaultCtor dependency)
            {
                if (dependency == null)
                    throw new ArgumentNullException("dependency");

                Dependency = dependency;
            }

            public TestClassWithDependency(TestClassDefaultCtor dependency, int param1, string param2)
            {
                Param1 = param1;
                Param2 = param2;
            }
        }

        internal class TestClassPrivateCtor
        {
            private TestClassPrivateCtor()
            {
            }
        }

        internal class TestClassWithParameters
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }

            public TestClassWithParameters(string stringProperty, int intProperty)
            {
                StringProperty = stringProperty;
                IntProperty = intProperty;
            }
        }

        internal class TestClassWithDependencyAndParameters
        {
            TestClassDefaultCtor Dependency { get; set; }

            public int Param1 { get; private set; }

            public string Param2 { get; private set; }

            public TestClassWithDependencyAndParameters(TestClassDefaultCtor dependency, int param1, string param2)
            {
                if (dependency == null)
                    throw new ArgumentNullException("dependency");

                Dependency = dependency;
                Param1 = param1;
                Param2 = param2;
            }
        }

        internal class TestClassNoInterfaceDefaultCtor
        {
            public TestClassNoInterfaceDefaultCtor()
            {

            }
        }

        internal class TestClassNoInterfaceDependency
        {
            public ITestInterface Dependency { get; set; }

            public TestClassNoInterfaceDependency(ITestInterface dependency)
            {
                if (dependency == null)
                    throw new ArgumentNullException("dependency");

                Dependency = dependency;
            }
        }

        public class DisposableTestClassWithInterface : IDisposable, ITestInterface
        {
            public void Dispose()
            {
                
            }
        }

        public class GenericClassWithInterface<I, S> : ITestInterface
        {
            public I Prop1 { get; set; }
            public S Prop2 { get; set; }

            public GenericClassWithInterface()
            {
                
            }

            private GenericClassWithInterface(I prop1, S prop2)
            {
                Prop1 = prop1;
                Prop2 = prop2;
            }
        }
    }
}
