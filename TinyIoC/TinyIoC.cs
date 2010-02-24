using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TinyIoC
{
    #region TinyIoC Exception Types
    public class TinyIoCResolutionException : Exception
    {
        private const string ERROR_TEXT = "Unable to resolve type: {0}";

        public TinyIoCResolutionException(Type type)
            : base(String.Format(ERROR_TEXT, type.FullName))
        {
        }

        public TinyIoCResolutionException(Type type, Exception innerException)
            : base(String.Format(ERROR_TEXT, type.FullName), innerException)
        {
        }
    }

    public class TinyIoCRegistrationException : Exception
    {
        private const string ERROR_TEXT = "Cannot convert current registration of {0} to {1}";

        public TinyIoCRegistrationException(Type type, string method)
            : base(String.Format(ERROR_TEXT, type.FullName, method))
        {
        }

        public TinyIoCRegistrationException(Type type, string method, Exception innerException)
            : base(String.Format(ERROR_TEXT, type.FullName, method), innerException)
        {
        }
    }
    #endregion

    public sealed class TinyIoC : IDisposable
    {
        #region Public Setup / Settings Classes
        /// <summary>
        /// Name/Value pairs for specifying "user" parameters when resolving
        /// </summary>
        public sealed class NamedParameterOverloads : Dictionary<string, object>
        {
            public static NamedParameterOverloads GetDefault()
            {
                return new NamedParameterOverloads();
            }
        }

        /// <summary>
        /// Resolution settings
        /// </summary>
        public sealed class ResolveOptions
        {
            public enum UnregisteredResolutionActions
            {
                AttemptResolve,
                Fail
            }

            public enum NamedResolutionFailureActions
            {
                AttemptUnnamedResolution,
                Fail
            }

            private UnregisteredResolutionActions _UnregisteredResolutionAction = UnregisteredResolutionActions.AttemptResolve;
            public UnregisteredResolutionActions UnregisteredResolutionAction
            {
                get { return _UnregisteredResolutionAction; }
                set { _UnregisteredResolutionAction = value; }
            }

            private NamedResolutionFailureActions _NamedResolutionFailureAction = NamedResolutionFailureActions.Fail;
            public NamedResolutionFailureActions NamedResolutionFailureAction
            {
                get { return _NamedResolutionFailureAction; }
                set { _NamedResolutionFailureAction = value; }
            }

            public ResolveOptions()
            {

            }

            /// <summary>
            /// Get default settings
            /// </summary>
            /// <returns>ResolveOptions instance with default settings</returns>
            public static ResolveOptions GetDefault()
            {
                return new ResolveOptions();
            }
        }

        /// <summary>
        /// Registration options for "fluent" API
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Implementation type for construction of RegisteredType</typeparam>
        public sealed class RegisterOptions<RegisterType, RegisterImplementation>
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            private TinyIoC _Container;

            public RegisterOptions(TinyIoC container)
            {
                _Container = container;
            }

            /// <summary>
            /// Make registration a singleton (single instance) if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions<RegisterType, RegisterImplementation> AsSingleton()
            {
                var currentFactory = _Container.GetCurrentFactory<RegisterType>();

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(typeof(RegisterType), "singleton");

                return _Container.AddUpdateRegistration<RegisterType, RegisterImplementation>(currentFactory.SingletonVariant);
            }

            /// <summary>
            /// Make registration multi-instance if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions<RegisterType, RegisterImplementation> AsMultiInstance()
            {
                var currentFactory = _Container.GetCurrentFactory<RegisterType>();

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(typeof(RegisterType), "multi-instance");

                return _Container.AddUpdateRegistration<RegisterType, RegisterImplementation>(currentFactory.MultiInstanceVariant);
            }

            /// <summary>
            /// Make registration hold a weak reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions<RegisterType, RegisterImplementation> WithWeakReference()
            {
                throw new NotImplementedException();

                var currentFactory = _Container.GetCurrentFactory<RegisterType>();

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(typeof(RegisterType), "weak reference");

                return _Container.AddUpdateRegistration<RegisterType, RegisterImplementation>(currentFactory.WeakReferenceVariant);
            }

            /// <summary>
            /// Make registration hold a strong reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions<RegisterType, RegisterImplementation> WithStrongReference()
            {
                throw new NotImplementedException();

                var currentFactory = _Container.GetCurrentFactory<RegisterType>();

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(typeof(RegisterType), "strong reference");

                return _Container.AddUpdateRegistration<RegisterType, RegisterImplementation>(currentFactory.StrongReferenceVariant);
            }
        }
        #endregion

        #region Public API
        #region Registration
        /// <summary>
        /// Creates/replaces a container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterImplementation, RegisterImplementation> Register<RegisterImplementation>()
            where RegisterImplementation : class
        {
            return Register<RegisterImplementation, RegisterImplementation>();
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterImplementation, RegisterImplementation> Register<RegisterImplementation>(string name)
            where RegisterImplementation : class
        {
            return Register<RegisterImplementation, RegisterImplementation>(name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterType, RegisterImplementation> Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return AddUpdateRegistration<RegisterType, RegisterImplementation>(GetDefaultObjectFactory<RegisterType, RegisterImplementation>());
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterType, RegisterImplementation> Register<RegisterType, RegisterImplementation>(string name)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return AddUpdateRegistration<RegisterType, RegisterImplementation>(GetDefaultObjectFactory<RegisterType, RegisterImplementation>(), name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterType, RegisterType> Register<RegisterType>(RegisterType instance)
           where RegisterType : class
        {
            return AddUpdateRegistration<RegisterType, RegisterType>(new InstanceFactory<RegisterType, RegisterType>(instance));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterType, RegisterType> Register<RegisterType>(RegisterType instance, string name)
            where RegisterType : class
        {
            return AddUpdateRegistration<RegisterType, RegisterType>(new InstanceFactory<RegisterType, RegisterType>(instance), name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterType, RegisterType> Register<RegisterType>(Func<TinyIoC, NamedParameterOverloads, RegisterType> factory)
            where RegisterType : class
        {
            return AddUpdateRegistration<RegisterType, RegisterType>(new DelegateFactory<RegisterType>(factory));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions<RegisterType, RegisterType> Register<RegisterType>(Func<TinyIoC, NamedParameterOverloads, RegisterType> factory, string name)
            where RegisterType : class
        {
            return AddUpdateRegistration<RegisterType, RegisterType>(new DelegateFactory<RegisterType>(factory), name);
        }
        #endregion

        #region Resolution
        /// <summary>
        /// Attempts to resolve a type using default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>()
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType)) as RegisterType);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>(ResolveOptions options)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), options) as RegisterType);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>(string name)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), name) as RegisterType);
        }

        /// <summary>
        /// Attempts to resolve a type using supplied options and  name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>(string name, ResolveOptions options)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), name, options) as RegisterType);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>(NamedParameterOverloads parameters)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), parameters) as RegisterType);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>(NamedParameterOverloads parameters, ResolveOptions options)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), parameters, options) as RegisterType);
        }

        public RegisterType Resolve<RegisterType>(string name, NamedParameterOverloads parameters, ResolveOptions options)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), name, parameters, options) as RegisterType);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters and name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="RegisterType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public RegisterType Resolve<RegisterType>(string name, NamedParameterOverloads parameters)
            where RegisterType : class
        {
            return (Resolve(typeof(RegisterType), name, parameters) as RegisterType);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>()
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType));
        }

        public bool CanResolve<ResolveType>(string name)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(ResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), options);
        }

        public bool CanResolve<ResolveType>(string name, ResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters and default options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(NamedParameterOverloads parameters)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), parameters);
        }

        public bool CanResolve<ResolveType>(string name, NamedParameterOverloads parameters)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name, parameters);
        }

        /// <summary>
        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(NamedParameterOverloads parameters, ResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), parameters, options);
        }

        public bool CanResolve<ResolveType>(string name, NamedParameterOverloads parameters, ResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name, parameters, options);
        }
        #endregion
        #endregion

        #region Object Factories
        private abstract class ObjectFactoryBase
        {
            /// <summary>
            /// Whether to assume this factory sucessfully constructs its objects
            /// 
            /// Generally set to true for delegate style factories as CanResolve cannot delve
            /// into the delegates they contain.
            /// </summary>
            public virtual bool AssumeConstruction { get { return false; } }

            /// <summary>
            /// The type the factory instantiates
            /// </summary>
            public abstract Type CreatesType { get; }

            /// <summary>
            /// Create the type
            /// </summary>
            /// <param name="container">Container that requested the creation</param>
            /// <param name="parameters">Any user parameters passed</param>
            /// <returns></returns>
            public abstract object GetObject(TinyIoC container, NamedParameterOverloads parameters, ResolveOptions options);

            public virtual ObjectFactoryBase SingletonVariant
            {
                get
                {
                    throw new TinyIoCRegistrationException(this.GetType(), "singleton");
                }
            }

            public virtual ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    throw new TinyIoCRegistrationException(this.GetType(), "multi-instance");
                }
            }

            public virtual ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    throw new TinyIoCRegistrationException(this.GetType(), "strong reference");
                }
            }

            public virtual ObjectFactoryBase WeakReferenceVariant
            {
                get
                {
                    throw new TinyIoCRegistrationException(this.GetType(), "weak reference");
                }
            }
        }

        /// <summary>
        /// IObjectFactory that creates new instances of types for each resolution
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Type to construct to fullful request for RegisteredType</typeparam>
        private class MultiInstanceFactory<RegisterType, RegisterImplementation> : ObjectFactoryBase
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            public override Type CreatesType { get { return typeof(RegisterImplementation); } }

            public override object GetObject(TinyIoC container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                try
                {
                    return container.ConstructType(typeof(RegisterImplementation), parameters, options);
                }
                catch (TinyIoCResolutionException ex)
                {
                    throw new TinyIoCResolutionException(typeof(RegisterImplementation), ex);
                }
            }

            public override ObjectFactoryBase SingletonVariant
            {
                get
                {
                    return new SingletonFactory<RegisterType, RegisterImplementation>();
                }
            }

            public override ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    return this;
                }
            }
        }

        /// <summary>
        /// IObjectFactory that invokes a specified delegate to construct the object
        /// </summary>
        /// <typeparam name="RegisterType">Registered type to be constructed</typeparam>
        private class DelegateFactory<RegisterType> : ObjectFactoryBase
            where RegisterType : class
        {
            private Func<TinyIoC, NamedParameterOverloads, RegisterType> _Factory;

            public override bool AssumeConstruction { get { return true; } }

            public override Type CreatesType { get { return typeof(RegisterType); } }

            public override object GetObject(TinyIoC container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                try
                {
                    return _Factory.Invoke(container, parameters);
                }
                catch (Exception ex)
                {
                    throw new TinyIoCResolutionException(typeof(RegisterType), ex);
                }
            }

            public DelegateFactory(Func<TinyIoC, NamedParameterOverloads, RegisterType> factory)
            {
                if (factory == null)
                    throw new ArgumentNullException("factory");

                _Factory = factory;
            }
        }

        /// <summary>
        /// Stores an particular instance to return for a type
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Type of the instance</typeparam>
        private class InstanceFactory<RegisterType, RegisterImplementation> : ObjectFactoryBase, IDisposable
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            private RegisterImplementation instance;

            public override Type CreatesType
            {
                get { return typeof(RegisterImplementation); }
            }

            public override object GetObject(TinyIoC container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                return instance;
            }

            public InstanceFactory(RegisterImplementation instance)
            {
                this.instance = instance;
            }

            public override ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    return new MultiInstanceFactory<RegisterType, RegisterImplementation>();
                }
            }

            public void Dispose()
            {
                var disposable = instance as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }
        }

        /// <summary>
        /// A factory that lazy instantiates a type and always returns the same instance
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate</typeparam>
        private class SingletonFactory<RegisterType, RegisterImplementation> : ObjectFactoryBase, IDisposable
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            private readonly object SingletonLock = new object();
            private RegisterImplementation _Current;

            public override Type CreatesType
            {
                get { return typeof(RegisterImplementation); }
            }

            public override object GetObject(TinyIoC container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                if (parameters.Count != 0)
                    throw new ArgumentException("Cannot specify parameters for singleton types");

                lock (SingletonLock)
                    if (_Current == null)
                        _Current = container.ConstructType(typeof(RegisterImplementation), options) as RegisterImplementation;

                return _Current;
            }

            public SingletonFactory()
            {

            }

            public override ObjectFactoryBase SingletonVariant
            {
                get
                {
                    return this;
                }
            }

            public override ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    return new MultiInstanceFactory<RegisterType, RegisterImplementation>();
                }
            }

            public void Dispose()
            {
                if (_Current != null)
                {
                    var disposable = _Current as IDisposable;

                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }
        #endregion

        #region Singleton Container
        private static readonly TinyIoC _Current = new TinyIoC();

        static TinyIoC()
        {
        }

        /// <summary>
        /// Lazy created Singleton instance of the container for simple scenarios
        /// </summary>
        public static TinyIoC Current
        {
            get
            {
                return _Current;
            }
        }
        #endregion

        #region Type Registrations
        private sealed class TypeRegistration
        {
            public Type Type { get; private set; }
            public string Name { get; private set; }

            public TypeRegistration(Type type)
                : this(type, string.Empty)
            {
            }

            public TypeRegistration(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            public override bool Equals(object obj)
            {
                var typeRegistration = obj as TypeRegistration;

                if (obj == null)
                    return false;

                if (this.Type != typeRegistration.Type)
                    return false;

                if (String.Compare(this.Name, typeRegistration.Name, true) != 0)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                return String.Format("{0}|{1}", this.Type.FullName, this.Name).GetHashCode();
            }
        }
        private readonly Dictionary<TypeRegistration, ObjectFactoryBase> _RegisteredTypes;
        #endregion

        #region Constructors
        public TinyIoC()
        {
            _RegisteredTypes = new Dictionary<TypeRegistration, ObjectFactoryBase>();

            RegisterDefaultTypes();
        }
        #endregion

        #region Internal Methods
        private void RegisterDefaultTypes()
        {
            this.Register<TinyIoC>(this);
        }

        private ObjectFactoryBase GetCurrentFactory<RegisterType>()
        {
            ObjectFactoryBase current = null;

            _RegisteredTypes.TryGetValue(new TypeRegistration(typeof(RegisterType)), out current);

            return current;
        }

        private RegisterOptions<RegisterType, RegisterImplementation> AddUpdateRegistration<RegisterType, RegisterImplementation>(ObjectFactoryBase factory)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return AddUpdateRegistration<RegisterType, RegisterImplementation>(factory, string.Empty);
        }

        private RegisterOptions<RegisterType, RegisterImplementation> AddUpdateRegistration<RegisterType, RegisterImplementation>(ObjectFactoryBase factory, string name)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            ObjectFactoryBase current;

            var typeRegistration = new TypeRegistration(typeof(RegisterType), name);

            if (_RegisteredTypes.TryGetValue(typeRegistration, out current))
            {
                var disposable = current as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }

            _RegisteredTypes[typeRegistration] = factory;

            return new RegisterOptions<RegisterType, RegisterImplementation>(this);
        }

        private static ObjectFactoryBase GetDefaultObjectFactory<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            if (typeof(RegisterType).IsInterface)
                return new SingletonFactory<RegisterType, RegisterImplementation>();

            return new MultiInstanceFactory<RegisterType, RegisterImplementation>();
        }

        private bool CanResolve(Type type)
        {
            return CanResolve(type, String.Empty, NamedParameterOverloads.GetDefault(), ResolveOptions.GetDefault());
        }

        private bool CanResolve(Type type, string name)
        {
            return CanResolve(type, name, NamedParameterOverloads.GetDefault(), ResolveOptions.GetDefault());
        }

        private bool CanResolve(Type type, ResolveOptions options)
        {
            return CanResolve(type, String.Empty, NamedParameterOverloads.GetDefault(), options);
        }

        private bool CanResolve(Type type, string name, ResolveOptions options)
        {
            return CanResolve(type, name, NamedParameterOverloads.GetDefault(), options);
        }

        private bool CanResolve(Type type, NamedParameterOverloads parameters)
        {
            return CanResolve(type, String.Empty, parameters, ResolveOptions.GetDefault());
        }

        private bool CanResolve(Type type, string name, NamedParameterOverloads parameters)
        {
            return CanResolve(type, name, parameters, ResolveOptions.GetDefault());
        }

        private bool CanResolve(Type type, NamedParameterOverloads parameters, ResolveOptions options)
        {
            return CanResolve(type, String.Empty, parameters, options);
        }

        private bool CanResolve(Type type, string name, NamedParameterOverloads parameters, ResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Type checkType = type;

            ObjectFactoryBase factory;
            if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType, name), out factory))
            {
                if (factory.AssumeConstruction)
                    return true;

                return (GetBestConstructor(factory.CreatesType, parameters, options) != null) ? true : false;
            }

            // Fail if requesting named resolution and settings set to fail if unresolved
            if (!String.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == ResolveOptions.NamedResolutionFailureActions.Fail)
                return false;

            // Attemped unnamed fallback container resolution if relevant and requested
            if (!String.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == ResolveOptions.NamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType), out factory))
                {
                    if (factory.AssumeConstruction)
                        return true;

                    return (GetBestConstructor(factory.CreatesType, parameters, options) != null) ? true : false;
                }
            }

            // Attempt unregistered construction if possible and requested
            if (options.UnregisteredResolutionAction == ResolveOptions.UnregisteredResolutionActions.AttemptResolve)
                return (GetBestConstructor(checkType, parameters, options) != null) ? true : false;

            return false;
        }

        private object Resolve(Type type)
        {
            return Resolve(type, string.Empty, NamedParameterOverloads.GetDefault(), ResolveOptions.GetDefault());
        }

        private object Resolve(Type type, ResolveOptions resolveOptions)
        {
            return Resolve(type, string.Empty, NamedParameterOverloads.GetDefault(), resolveOptions);
        }

        private object Resolve(Type type, string name)
        {
            return Resolve(type, name, NamedParameterOverloads.GetDefault(), ResolveOptions.GetDefault());
        }

        private object Resolve(Type type, string name, ResolveOptions options)
        {
            return Resolve(type, name, NamedParameterOverloads.GetDefault(), options);
        }

        private object Resolve(Type type, NamedParameterOverloads parameters)
        {
            return Resolve(type, string.Empty, parameters, ResolveOptions.GetDefault());
        }

        private object Resolve(Type type, NamedParameterOverloads parameters, ResolveOptions options)
        {
            return Resolve(type, string.Empty, parameters, options);
        }

        private object Resolve(Type type, string name, NamedParameterOverloads parameters)
        {
            return Resolve(type, name, parameters, ResolveOptions.GetDefault());
        }

        private object Resolve(Type type, string name, NamedParameterOverloads parameters, ResolveOptions options)
        {
            ObjectFactoryBase factory;

            // Attempt container resolution
            if (_RegisteredTypes.TryGetValue(new TypeRegistration(type, name), out factory))
            {
                try
                {
                    return factory.GetObject(this, parameters, options);
                }
                catch (Exception ex)
                {
                    throw new TinyIoCResolutionException(type, ex);
                }
            }

            // Fail if requesting named resolution and settings set to fail if unresolved
            if (!String.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == ResolveOptions.NamedResolutionFailureActions.Fail)
                throw new TinyIoCResolutionException(type);

            // Attemped unnamed fallback container resolution if relevant and requested
            if (!String.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == ResolveOptions.NamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(type, string.Empty), out factory))
                {
                    try
                    {
                        return factory.GetObject(this, parameters, options);
                    }
                    catch (Exception ex)
                    {
                        throw new TinyIoCResolutionException(type, ex);
                    }
                }
            }

            // Attempt unregistered construction if possible and requested
            if (options.UnregisteredResolutionAction == ResolveOptions.UnregisteredResolutionActions.AttemptResolve)
            {
                if (!type.IsAbstract && !type.IsInterface)
                    return ConstructType(type, parameters, options);
            }

            // Unable to resolve - throw
            throw new TinyIoCResolutionException(type);
        }

        private bool CanConstruct(ConstructorInfo ctor, NamedParameterOverloads parameters, ResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            foreach (var parameter in ctor.GetParameters())
            {
                // TODO - Find a better way of fixing - shouldn't really get this far with ctors with nameless params?
                if (string.IsNullOrEmpty(parameter.Name))
                    return false;

                if (!parameters.ContainsKey(parameter.Name) && !CanResolve(parameter.ParameterType, options))
                    return false;
            }

            return true;
        }

        private ConstructorInfo GetBestConstructor(Type type, NamedParameterOverloads parameters, ResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (type.IsValueType)
                return null;

            var ctors = from ctor in type.GetConstructors()
                        orderby ctor.GetParameters().Count()
                        select ctor;

            foreach (var ctor in ctors)
            {
                if (CanConstruct(ctor, parameters, options))
                    return ctor;
            }

            return null;
        }

        private object ConstructType(Type type, ResolveOptions options)
        {
            return ConstructType(type, NamedParameterOverloads.GetDefault(), options);
        }

        private object ConstructType(Type type, NamedParameterOverloads parameters, ResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            var ctor = GetBestConstructor(type, parameters, options);
            if (ctor == null)
                throw new TinyIoCResolutionException(type);

            var ctorParams = ctor.GetParameters();
            object[] args = new object[ctorParams.Count()];

            for (int parameterIndex = 0; parameterIndex < ctorParams.Count(); parameterIndex++)
            {
                var currentParam = ctorParams[parameterIndex];

                args[parameterIndex] = parameters.ContainsKey(currentParam.Name) ? parameters[currentParam.Name] : Resolve(currentParam.ParameterType, options);
            }

            try
            {
                return Activator.CreateInstance(type, args);
            }
            catch (Exception ex)
            {
                throw new TinyIoCResolutionException(type, ex);
            }
        }

        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            var disposableFactories = from factory in _RegisteredTypes.Values
                                      where factory is IDisposable
                                      select factory as IDisposable;

            foreach (var factory in disposableFactories)
            {
                factory.Dispose();
            }

            _RegisteredTypes.Clear();
        }

        #endregion
    }
}
