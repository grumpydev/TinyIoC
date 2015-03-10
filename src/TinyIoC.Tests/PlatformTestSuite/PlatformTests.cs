#region Preprocessor Directives
// Uncomment this line if you want the container to automatically
// register the TinyMessenger messenger/event aggregator
//#define TINYMESSENGER

// Uncomment this line if you want to internalize this library
//#define TINYIOC_INTERNAL

// Uncomment this line if you want to target PCL.
//#define PORTABLE

// Preprocessor directives for enabling/disabling functionality
// depending on platform features. If the platform has an appropriate
// #DEFINE then these should be set automatically below.
#define EXPRESSIONS                         // Platform supports System.Linq.Expressions
#define COMPILED_EXPRESSIONS                // Platform supports compiling expressions
#define APPDOMAIN_GETASSEMBLIES             // Platform supports getting all assemblies from the AppDomain object
#define UNBOUND_GENERICS_GETCONSTRUCTORS    // Platform supports GetConstructors on unbound generic types
#define GETPARAMETERS_OPEN_GENERICS         // Platform supports GetParameters on open generics
#define RESOLVE_OPEN_GENERICS               // Platform supports resolving open generics
#define READER_WRITER_LOCK_SLIM             // Platform supports ReaderWriterLockSlim

#if PORTABLE
#undef APPDOMAIN_GETASSEMBLIES
#undef COMPILED_EXPRESSIONS
#undef READER_WRITER_LOCK_SLIM
#endif

// CompactFramework / Windows Phone 7
// By default does not support System.Linq.Expressions.
// AppDomain object does not support enumerating all assemblies in the app domain.
#if PocketPC || WINDOWS_PHONE
#undef EXPRESSIONS
#undef COMPILED_EXPRESSIONS
#undef APPDOMAIN_GETASSEMBLIES
#undef UNBOUND_GENERICS_GETCONSTRUCTORS
#endif

// PocketPC has a bizarre limitation on enumerating parameters on unbound generic methods.
// We need to use a slower workaround in that case.
#if PocketPC
#undef GETPARAMETERS_OPEN_GENERICS
#undef RESOLVE_OPEN_GENERICS
#undef READER_WRITER_LOCK_SLIM
#endif

#if SILVERLIGHT
#undef APPDOMAIN_GETASSEMBLIES
#endif

#if NETFX_CORE
#undef APPDOMAIN_GETASSEMBLIES
#undef RESOLVE_OPEN_GENERICS
#endif

#if COMPILED_EXPRESSIONS
#define USE_OBJECT_CONSTRUCTOR
#endif

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TinyIoC.Tests.PlatformTestSuite
{
    public interface ILogger
    {
        void WriteLine(string text);
    }

    public class StringLogger : ILogger
    {
        private readonly StringBuilder builder = new StringBuilder();

        public string Log
        {
            get { return builder.ToString(); }
        }


        public void WriteLine(string text)
        {
            builder.AppendLine(text);
        }
    }

    public class PlatformTests
    {
        #region TestClasses
        public class TestClassNoInterface
        {
        }

        public interface ITestInterface
        {
        }

        public interface ITestInterface2
        {
        }

        public class TestClassWithInterface : ITestInterface
        {
        }

        public class TestClassWithInterface2 : ITestInterface
        {
        }

        public class TestClassWithConcreteDependency
        {
            public TestClassNoInterface Dependency { get; set; }

            public TestClassWithConcreteDependency(TestClassNoInterface dependency)
            {
                Dependency = dependency;
            }

            public TestClassWithConcreteDependency()
            {

            }
        }

        public class TestClassWithInterfaceDependency
        {
            public ITestInterface Dependency { get; set; }

            public TestClassWithInterfaceDependency(ITestInterface dependency)
            {
                Dependency = dependency;
            }
        }

        public class TestClassWithParameters
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }

            public TestClassWithParameters(string stringProperty, int intProperty)
            {
                StringProperty = stringProperty;
                IntProperty = intProperty;
            }
        }

        public class GenericClass<T>
        {

        }

        public class TestClassWithLazyFactory
        {
            private Func<TestClassNoInterface> _Factory;
            public TestClassNoInterface Prop1 { get; private set; }
            public TestClassNoInterface Prop2 { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestClassWithLazyFactory class.
            /// </summary>
            /// <param name="factory"></param>
            public TestClassWithLazyFactory(Func<TestClassNoInterface> factory)
            {
                _Factory = factory;
                Prop1 = _Factory.Invoke();
                Prop2 = _Factory.Invoke();
            }
        }

        public class TestClassWithNamedLazyFactory
        {
            private Func<string, TestClassNoInterface> _Factory;
            public TestClassNoInterface Prop1 { get; private set; }
            public TestClassNoInterface Prop2 { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestClassWithLazyFactory class.
            /// </summary>
            /// <param name="factory"></param>
            public TestClassWithNamedLazyFactory(Func<string, TestClassNoInterface> factory)
            {
                _Factory = factory;
                Prop1 = _Factory.Invoke("Testing");
                Prop2 = _Factory.Invoke("Testing");
            }
        }

        internal class TestclassWithNameAndParamsLazyFactory
        {
            private Func<string, IDictionary<String, object>, TestClassWithParameters> _Factory;
            public TestClassWithParameters Prop1 { get; private set; }

            public TestclassWithNameAndParamsLazyFactory(Func<string, IDictionary<String, object>, TestClassWithParameters> factory)
            {
                _Factory = factory;
                Prop1 = _Factory.Invoke("Testing", new Dictionary<String, object> { { "stringProperty", "Testing" }, { "intProperty", 22 } });
            }
        }

        internal class TestClassEnumerableDependency
        {
            IEnumerable<ITestInterface> _Enumerable;

            public int EnumerableCount { get { return _Enumerable == null ? 0 : _Enumerable.Count(); } }

            public TestClassEnumerableDependency(IEnumerable<ITestInterface> enumerable)
            {
                _Enumerable = enumerable;
            }
        }

        public interface IThing<T> where T : new()
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
        #endregion


        private ILogger _Logger;
        private int _TestsRun;
        private int _TestsPassed;
        private int _TestsFailed;

        private List<Func<TinyIoC.TinyIoCContainer, ILogger, bool>> _Tests;

        public PlatformTests(ILogger logger)
        {
            _Logger = logger;

            _Tests = new List<Func<TinyIoC.TinyIoCContainer, ILogger, bool>>()
            {
#if APPDOMAIN_GETASSEMBLIES
                AutoRegisterAppDomain,
#endif
                AutoRegisterAssemblySpecified,
                AutoRegisterPredicateExclusion,
                RegisterConcrete,
                ResolveConcreteUnregisteredDefaultOptions,
                ResolveConcreteRegisteredDefaultOptions,
                RegisterNamedConcrete,
                ResolveNamedConcrete,
                RegisterInstance,
                RegisterInterface,
                RegisterStrongRef,
                RegisterWeakRef,
                RegisterFactory,
#if EXPRESSIONS
                RegisterAndSpecifyConstructor,
#endif
                RegisterBoundGeneric,
#if EXPRESSIONS
                ResolveLazyFactory,
                ResolveNamedLazyFactory,
                ResolveNamedAndParamsLazyFactory,
#endif
                ResolveAll,
                IEnumerableDependency,
                RegisterMultiple,
                NonGenericRegister,
#if RESOLVE_OPEN_GENERICS 
                OpenGenericRegistration,
                OpenGenericResolution,
                OpenGenericCanResolve
#endif
            };
        }

        public void RunTests(out int testsRun, out int testsPassed, out int testsFailed)
        {
            _TestsRun = 0;
            _TestsPassed = 0;
            _TestsFailed = 0;

            foreach (var test in _Tests)
            {
                var container = GetContainer();
                try
                {
                    _TestsRun++;
                    if (test.Invoke(container, _Logger))
                        _TestsPassed++;
                    else
                    {
                        _TestsFailed++;
                        _Logger.WriteLine("Test Failed");
                    }
                }
                catch (Exception ex)
                {
                    _TestsFailed++;
                    _Logger.WriteLine(String.Format("Test Failed: {0}", ex.Message));
                }
            }

            testsRun = _TestsRun;
            testsPassed = _TestsPassed;
            testsFailed = _TestsFailed;
        }

        private TinyIoC.TinyIoCContainer GetContainer()
        {
            return new TinyIoCContainer();
        }

        private bool AutoRegisterAppDomain(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("AutoRegisterAppDomain");
            container.AutoRegister();
            container.Resolve<ITestInterface>();
            return true;
        }

        private bool AutoRegisterAssemblySpecified(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("AutoRegisterAssemblySpecified");
            container.AutoRegister(new[] { this.GetType().Assembly });
            container.Resolve<ITestInterface>();
            return true;
        }

        private bool RegisterConcrete(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterConcrete");
            container.Register<TestClassNoInterface>();
            container.Resolve<TestClassNoInterface>();
            return true;
        }

        private bool ResolveConcreteUnregisteredDefaultOptions(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveConcreteUnregisteredDefaultOptions");
            var output = container.Resolve<TestClassNoInterface>();

            return output is TestClassNoInterface;
        }

        private bool ResolveConcreteRegisteredDefaultOptions(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveConcreteRegisteredDefaultOptions");
            container.Register<TestClassNoInterface>();
            var output = container.Resolve<TestClassNoInterface>();

            return output is TestClassNoInterface;
        }

        private bool RegisterNamedConcrete(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterNamedConcrete");
            container.Register<TestClassNoInterface>("Testing");
            var output = container.Resolve<TestClassNoInterface>("Testing");

            return output is TestClassNoInterface;
        }

        private bool ResolveNamedConcrete(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveNamedConcrete");
            container.Register<TestClassNoInterface>("Testing");
            var output = container.Resolve<TestClassNoInterface>("Testing");

            return output is TestClassNoInterface;
        }

        private bool RegisterInstance(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterInstance");
            var obj = new TestClassNoInterface();
            container.Register<TestClassNoInterface>(obj);
            var output = container.Resolve<TestClassNoInterface>();
            return object.ReferenceEquals(obj, output);
        }

        private bool RegisterInterface(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterInterface");
            container.Register<ITestInterface, TestClassWithInterface>();
            var output = container.Resolve<ITestInterface>();
            return output is ITestInterface;
        }

        private bool RegisterStrongRef(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterStrongRef");
            var obj = new TestClassNoInterface();
            container.Register<TestClassNoInterface>(obj).WithStrongReference();
            var output = container.Resolve<TestClassNoInterface>();

            return output is TestClassNoInterface;
        }

        private bool RegisterWeakRef(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterWeakRef");
            var obj = new TestClassNoInterface();
            container.Register<TestClassNoInterface>(obj).WithWeakReference();
            var output = container.Resolve<TestClassNoInterface>();

            return output is TestClassNoInterface;
        }

        private bool RegisterFactory(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterFactory");
            container.Register<TestClassNoInterface>((c, p) => new TestClassNoInterface());
            var output = container.Resolve<TestClassNoInterface>();

            return output is TestClassNoInterface;
        }

#if EXPRESSIONS
        private bool RegisterAndSpecifyConstructor(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterAndSpecifyConstructor");
            container.Register<TestClassWithConcreteDependency>().UsingConstructor(() => new TestClassWithConcreteDependency());
            var output = container.Resolve<TestClassWithConcreteDependency>();

            return output is TestClassWithConcreteDependency;
        }
#endif

        private bool RegisterBoundGeneric(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterBoundGeneric");
            container.Register<GenericClass<String>>();
            var output = container.Resolve<GenericClass<String>>();

            return output is GenericClass<String>;
        }

#if EXPRESSIONS
        private bool ResolveLazyFactory(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveLazyFactory");
            container.Register<TestClassNoInterface>();
            container.Register<TestClassWithLazyFactory>();
            var output = container.Resolve<TestClassWithLazyFactory>();
            return (output.Prop1 != null) && (output.Prop2 != null);
        }

        private bool ResolveNamedLazyFactory(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveNamedLazyFactory");
            container.Register<TestClassNoInterface>("Testing");
            container.Register<TestClassWithNamedLazyFactory>();
            var output = container.Resolve<TestClassWithNamedLazyFactory>();
            return (output.Prop1 != null) && (output.Prop2 != null);
        }

        private bool ResolveNamedAndParamsLazyFactory(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveNamedAndParamsLazyFactory");
            container.Register<TestClassWithParameters>("Testing");
            container.Register<TestclassWithNameAndParamsLazyFactory>();
            var output = container.Resolve<TestclassWithNameAndParamsLazyFactory>();
            return (output.Prop1 != null);
        }

#endif

        private bool ResolveAll(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("ResolveAll");
            container.Register<ITestInterface, TestClassWithInterface>();
            container.Register<ITestInterface, TestClassWithInterface>("Named1");
            container.Register<ITestInterface, TestClassWithInterface>("Named2");

            IEnumerable<ITestInterface> result = container.ResolveAll<ITestInterface>();

            return (result.Count() == 3);
        }

        private bool IEnumerableDependency(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("IEnumerableDependency");
            container.Register<ITestInterface, TestClassWithInterface>();
            container.Register<ITestInterface, TestClassWithInterface>("Named1");
            container.Register<ITestInterface, TestClassWithInterface>("Named2");
            container.Register<TestClassEnumerableDependency>();

            var result = container.Resolve<TestClassEnumerableDependency>();

            return (result.EnumerableCount == 2);
        }

        private bool RegisterMultiple(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("RegisterMultiple");
            container.RegisterMultiple<ITestInterface>(new Type[] { typeof(TestClassWithInterface), typeof(TestClassWithInterface2) });

            var result = container.ResolveAll<ITestInterface>();

            return (result.Count() == 2);
        }

        private bool NonGenericRegister(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("NonGenericRegister");
            container.Register(typeof(ITestInterface), typeof(TestClassWithInterface));

            var result = container.Resolve<ITestInterface>(ResolveOptions.FailUnregisteredAndNameNotFound);

            return true;
        }

        private bool AutoRegisterPredicateExclusion(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("AutoRegisterPredicateExclusion");
            container.AutoRegister(t => t != typeof(ITestInterface));

            try
            {
                container.Resolve<ITestInterface>();
                return false;
            }
            catch (TinyIoCResolutionException)
            {
            }

            return true;
        }

        private bool OpenGenericRegistration(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("OpenGenericRegistration");

            container.Register(typeof(IThing<>), typeof(DefaultThing<>));

            return true;
        }

        private bool OpenGenericResolution(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("OpenGenericResolution");

            container.Register(typeof(IThing<>), typeof(DefaultThing<>));

            var result = container.Resolve<IThing<object>>();

            return result != null && result.GetType() == typeof(DefaultThing<object>);
        }

        private bool OpenGenericCanResolve(TinyIoCContainer container, ILogger logger)
        {
            logger.WriteLine("OpenGenericCanResolve");

            container.Register(typeof(IThing<>), typeof(DefaultThing<>));

            return container.CanResolve(typeof(IThing<int>));
        }
    }
}
