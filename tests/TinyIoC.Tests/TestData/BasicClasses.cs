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

namespace TinyIoC.Tests.TestData
{
    namespace BasicClasses
    {
        internal interface ITestInterface
        {
        }

        internal interface ITestInterface<I, S>
        {
        }

        internal class TestClassDefaultCtor : ITestInterface
        {
            public string Prop1 { get; set; }

            public TestClassDefaultCtor()
            {

            }

            public static ITestInterface CreateNew(TinyIoCContainer container)
            {
                return new TestClassDefaultCtor() { Prop1 = "Testing" };
            }
        }

        internal interface ITestInterface2
        {
        }

        internal class TestClass2 : ITestInterface2
        {
        }

        internal class TestClassWithContainerDependency
        {
            public TinyIoCContainer _Container { get; private set; }

            public TestClassWithContainerDependency(TinyIoCContainer container)
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

            public GenericClassWithInterface(I prop1, S prop2)
            {
                Prop1 = prop1;
                Prop2 = prop2;
            }
        }

        internal class GenericClassWithGenericInterface<I, S> : ITestInterface<I, S>
        {
            public I Prop1 { get; set; }
            public S Prop2 { get; set; }

             public GenericClassWithGenericInterface()
             {
             }

             public GenericClassWithGenericInterface(I prop1, S prop2)
            {
                Prop1 = prop1;
                Prop2 = prop2;
            }
        }

        internal class GenericClassWithParametersAndDependencies<I, S>
        {
            public ITestInterface2 Dependency { get; private set; }
            public I Prop1 { get; set; }
            public S Prop2 { get; set; }

            public GenericClassWithParametersAndDependencies(ITestInterface2 dependency)
            {
                Dependency = dependency;
            }

            public GenericClassWithParametersAndDependencies(ITestInterface2 dependency, I prop1, S prop2)
            {
                Dependency = dependency;
                Prop1 = prop1;
                Prop2 = prop2;
            }
        }

        internal class TestClassWithLazyFactory
        {
            private Func<TestClassDefaultCtor> _Factory;
            public TestClassDefaultCtor Prop1 { get; private set; }
            public TestClassDefaultCtor Prop2 { get; private set; }
            
            /// <summary>
            /// Initializes a new instance of the TestClassWithLazyFactory class.
            /// </summary>
            /// <param name="factory"></param>
            public TestClassWithLazyFactory(Func<TestClassDefaultCtor> factory)
            {
                _Factory = factory;
            }

            public void Method1()
            {
                Prop1 = _Factory.Invoke();
            }

            public void Method2()
            {
                Prop2 = _Factory.Invoke();
            }

        }

        internal class TestClassWithNamedLazyFactory
        {
            private Func<string, TestClassDefaultCtor> _Factory;
            public TestClassDefaultCtor Prop1 { get; private set; }
            public TestClassDefaultCtor Prop2 { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestClassWithLazyFactory class.
            /// </summary>
            /// <param name="factory"></param>
            public TestClassWithNamedLazyFactory(Func<string, TestClassDefaultCtor> factory)
            {
                _Factory = factory;
            }

            public void Method1()
            {
                Prop1 = _Factory.Invoke("Testing");
            }

            public void Method2()
            {
                Prop2 = _Factory.Invoke(String.Empty);
            }

        }

        internal class TestclassWithNameAndParamsLazyFactory
        {
            private Func<string, IDictionary<String, object>, TestClassWithParameters> _Factory;
            public TestClassWithParameters Prop1 { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestclassWithNameAndParamsLazyFactory class.
            /// </summary>
            /// <param name="factory"></param>
            public TestclassWithNameAndParamsLazyFactory(Func<string, IDictionary<String, object>, TestClassWithParameters> factory)
            {
                _Factory = factory;
                Prop1 = _Factory.Invoke("", new Dictionary<String, object> { { "stringProperty", "Testing" }, { "intProperty", 22 } });
            }

        }

        internal class TestClassMultiDepsMultiCtors
        {
            public TestClassDefaultCtor Prop1 { get; private set; }
            public TestClassDefaultCtor Prop2 { get; private set; }
            public int NumberOfDepsResolved { get; private set; }

            public TestClassMultiDepsMultiCtors(TestClassDefaultCtor prop1)
            {
                Prop1 = prop1;
                NumberOfDepsResolved = 1;
            }

            public TestClassMultiDepsMultiCtors(TestClassDefaultCtor prop1, TestClassDefaultCtor prop2)
            {
                Prop1 = prop1;
                Prop2 = prop2;
                NumberOfDepsResolved = 2;
            }
        }

        internal class TestClassConstructorFailure
        {
            /// <summary>
            /// Initializes a new instance of the TestClassConstructorFailure class.
            /// </summary>
            public TestClassConstructorFailure()
            {
                throw new NotImplementedException();
            }
       }

        internal abstract class TestClassBase
        {
        }

        internal class TestClassWithBaseClass : TestClassBase
        {

        }

        internal class TestClassPropertyDependencies
        {
            public ITestInterface Property1 { get; set; }
            public ITestInterface2 Property2 { get; set; }
            public int Property3 { get; set; }
            public string Property4 { get; set; }

            public TestClassDefaultCtor ConcreteProperty { get; set; }

            public ITestInterface ReadOnlyProperty { get; private set; }
            public ITestInterface2 WriteOnlyProperty { internal get; set; }

            /// <summary>
            /// Initializes a new instance of the TestClassPropertyDependencies class.
            /// </summary>
            public TestClassPropertyDependencies()
            {
                
            }
        }

        internal class TestClassEnumerableDependency
        {
            public IEnumerable<ITestInterface> Enumerable {get; private set;}

            public int EnumerableCount { get { return Enumerable == null ? 0 : Enumerable.Count(); } }

            public TestClassEnumerableDependency(IEnumerable<ITestInterface> enumerable)
            {
                Enumerable = enumerable;
            }
        }

        internal class TestClassEnumerableDependency2
        {
            public IEnumerable<ITestInterface2> Enumerable { get; private set; }

            public int EnumerableCount { get { return Enumerable == null ? 0 : Enumerable.Count(); } }

            public TestClassEnumerableDependency2(IEnumerable<ITestInterface2> enumerable)
            {
                Enumerable = enumerable;
            }
        }

        public interface IThing<T> where T: new()
        {
            T Get();
        }

        public class DefaultThing<T> : IThing<T> where T : new()
        {
            public T Get()
            {
                return new T();
            }
        }

        internal class TestClassWithConstructorAttrib
        {
            [TinyIoCConstructor]
            public TestClassWithConstructorAttrib()
            {
                AttributeConstructorUsed = true;
            }

            public TestClassWithConstructorAttrib(object someParameter)
            {
                AttributeConstructorUsed = false;
            }

            public bool AttributeConstructorUsed { get; private set; }
        }

        internal class TestClassWithInternalConstructorAttrib
        {
            [TinyIoCConstructor]
            internal TestClassWithInternalConstructorAttrib()
            {
                AttributeConstructorUsed = true;
            }

            public TestClassWithInternalConstructorAttrib(object someParameter)
            {
                AttributeConstructorUsed = false;
            }

            public bool AttributeConstructorUsed { get; private set; }
        }

        internal class TestClassWithManyConstructorAttribs
        {
            [TinyIoCConstructor]
            public TestClassWithManyConstructorAttribs()
            {
                MostGreedyAttribCtorUsed = false;
            }

            [TinyIoCConstructor]
            public TestClassWithManyConstructorAttribs(object someParameter)
            {
                MostGreedyAttribCtorUsed = true;
            }

            public TestClassWithManyConstructorAttribs(object a, object b)
            {
                MostGreedyAttribCtorUsed = false;
            }

            public bool MostGreedyAttribCtorUsed { get; private set; }
        }
    }
}
