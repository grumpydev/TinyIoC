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
using System.Linq.Expressions;

namespace TinyIoC
{
    #region SafeDictionary
    public class SafeDictionary<TKey, TValue> : IDisposable
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            set
            {
                lock (_Padlock)
                {
                    TValue current;
                    if (_Dictionary.TryGetValue(key, out current))
                    {
                        var disposable = current as IDisposable;

                        if (disposable != null)
                            disposable.Dispose();
                    }

                    _Dictionary[key] = value;
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_Padlock)
            {
                return _Dictionary.TryGetValue(key, out value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (_Padlock)
            {
                return _Dictionary.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_Padlock)
            {
                _Dictionary.Clear();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_Padlock)
            {
                var disposableItems = from item in _Dictionary.Values
                                      where item is IDisposable
                                      select item as IDisposable;

                foreach (var item in disposableItems)
                {
                    item.Dispose();
                }
            }
        }

        #endregion
    }
    #endregion

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

    public class TinyIoCRegistrationTypeExceptiopn : Exception
    {
        private const string REGISTER_ERROR_TEXT = "Cannot register type {0} - abstract classes or interfaces are not valid implementation types for {1}.";

        public TinyIoCRegistrationTypeExceptiopn(Type type, string factory)
            : base(String.Format(REGISTER_ERROR_TEXT, type.FullName, factory))
        {
        }

        public TinyIoCRegistrationTypeExceptiopn(Type type, string factory, Exception innerException)
            : base(String.Format(REGISTER_ERROR_TEXT, type.FullName, factory), innerException)
        {
        }
    }

    public class TinyIoCRegistrationException : Exception
    {
        private const string CONVERT_ERROR_TEXT = "Cannot convert current registration of {0} to {1}";

        public TinyIoCRegistrationException(Type type, string method)
            : base(String.Format(CONVERT_ERROR_TEXT, type.FullName, method))
        {
        }

        public TinyIoCRegistrationException(Type type, string method, Exception innerException)
            : base(String.Format(CONVERT_ERROR_TEXT, type.FullName, method), innerException)
        {
        }
    }

    public class TinyIoCWeakReferenceException : Exception
    {
        private const string ERROR_TEXT = "Unable to instantiate {0} - referenced object has been reclaimed";

        public TinyIoCWeakReferenceException(Type type)
            : base(String.Format(ERROR_TEXT, type.FullName))
        {
        }

        public TinyIoCWeakReferenceException(Type type, Exception innerException)
            : base(String.Format(ERROR_TEXT, type.FullName), innerException)
        {
        }
    }

    public class TinyIoCConstructorResolutionException : Exception
    {
        private const string ERROR_TEXT = "Unable to resolve constructor for {0} using provided Expression.";

        public TinyIoCConstructorResolutionException(Type type)
            : base(String.Format(ERROR_TEXT, type.FullName))
        {
        }

        public TinyIoCConstructorResolutionException(Type type, Exception innerException)
            : base(String.Format(ERROR_TEXT, type.FullName), innerException)
        {
        }

        public TinyIoCConstructorResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TinyIoCConstructorResolutionException(string message)
            : base(message)
        {
        }
    }
    #endregion

    #region Public Setup / Settings Classes
    /// <summary>
    /// Name/Value pairs for specifying "user" parameters when resolving
    /// </summary>
    public sealed class NamedParameterOverloads : Dictionary<string, object>
    {
        private static readonly NamedParameterOverloads _Default = new NamedParameterOverloads();

        public static NamedParameterOverloads Default
        {
            get
            {
                return _Default;
            }
        }
    }

    public enum UnregisteredResolutionActions
    {
        /// <summary>
        /// Attempt to resolve type, even if the type isn't registered.
        /// 
        /// Registered types/options will always take precedence.
        /// </summary>
        AttemptResolve,

        /// <summary>
        /// Fail resolution if type not explicitly registered
        /// </summary>
        Fail,

        /// <summary>
        /// Attempt to resolve unregistered type if requested type is generic
        /// and no registration exists for the specific generic parameters used.
        /// 
        /// Registered types/options will always take precedence.
        /// </summary>
        GenericsOnly
    }

    public enum NamedResolutionFailureActions
    {
        AttemptUnnamedResolution,
        Fail
    }

    /// <summary>
    /// Resolution settings
    /// </summary>
    public sealed class ResolveOptions
    {
        private static readonly ResolveOptions _Default = new ResolveOptions();

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

        public static ResolveOptions Default
        {
            get
            {
                return _Default;
            }
        }
    }
    #endregion

    public sealed class TinyIoCContainer : IDisposable
    {
        #region "Fluent" API
        /// <summary>
        /// Registration options for "fluent" API
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Implementation type for construction of RegisteredType</typeparam>
        public sealed class RegisterOptions
        {
            private TinyIoCContainer _Container;
            private TypeRegistration _Registration;

            public RegisterOptions(TinyIoCContainer container, TypeRegistration registration)
            {
                _Container = container;
                _Registration = registration;
            }

            /// <summary>
            /// Make registration a singleton (single instance) if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions AsSingleton()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(_Registration.Type, "singleton");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.SingletonVariant);
            }

            /// <summary>
            /// Make registration multi-instance if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions AsMultiInstance()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(_Registration.Type, "multi-instance");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.MultiInstanceVariant);
            }

            /// <summary>
            /// Make registration hold a weak reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions WithWeakReference()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(_Registration.Type, "weak reference");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.WeakReferenceVariant);
            }

            /// <summary>
            /// Make registration hold a strong reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="TinyIoCInstantiationTypeException"></exception>
            public RegisterOptions WithStrongReference()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new TinyIoCRegistrationException(_Registration.Type, "strong reference");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.StrongReferenceVariant);
            }

            public RegisterOptions UsingConstructor<RegisterType>(Expression<Func<RegisterType>> constructor)
            {
                var lambda = constructor as LambdaExpression;
                if (lambda == null)
                    throw new TinyIoCConstructorResolutionException(typeof(RegisterType));

                var newExpression = lambda.Body as NewExpression;
                if (newExpression == null)
                    throw new TinyIoCConstructorResolutionException(typeof(RegisterType));

                var constructorInfo = newExpression.Constructor;
                if (constructorInfo == null)
                    throw new TinyIoCConstructorResolutionException(typeof(RegisterType));

                var currentFactory = _Container.GetCurrentFactory(_Registration);
                if (currentFactory == null)
                    throw new TinyIoCConstructorResolutionException(typeof(RegisterType));

                currentFactory.SetConstructor(constructorInfo);

                return this;
            }
        }
        #endregion

        #region Public API
        #region Registration
        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the assembly containing TinyIoC.
        /// 
        /// If more than one class implements an interface then only one implementation will be registered
        /// although no error will be thrown.
        /// </summary>
        public void AutoRegister()
        {
            AutoRegister(this.GetType().Assembly);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assembly
        /// 
        /// If more than one class implements an interface then only one implementation will be registered
        /// although no error will be thrown.
        /// </summary>
        /// <param name="assembly">Assembly to process</param>
        public void AutoRegister(Assembly assembly)
        {
            var abstractInterfaceImplementations = new Dictionary<Type, Type>();

            var defaultFactoryMethod = this.GetType().GetMethod("GetDefaultObjectFactory", BindingFlags.NonPublic | BindingFlags.Instance);

            var concreteTypes = from type in assembly.GetTypes()
                                where (type.IsClass == true) && (type.IsAbstract == false) && (type != this.GetType() && (type.DeclaringType != this.GetType()) && (!type.IsGenericTypeDefinition))
                                select type;

            foreach (var type in concreteTypes)
            {
                if (type.BaseType != typeof(object))
                    abstractInterfaceImplementations[type.BaseType] = type;

                foreach (var interfaceType in type.GetInterfaces())
                {
                    abstractInterfaceImplementations[interfaceType] = type;
                }

                Type[] genericTypes = { type, type };
                var genericDefaultFactoryMethod = defaultFactoryMethod.MakeGenericMethod(genericTypes);
                this.RegisterInternal(type, type, string.Empty, genericDefaultFactoryMethod.Invoke(this, null) as ObjectFactoryBase);
            }

            var abstractInterfaceTypes = from type in assembly.GetTypes()
                                 where ((type.IsInterface == true || type.IsAbstract == true) && (type.DeclaringType != this.GetType()) && (!type.IsGenericTypeDefinition))
                                 select type;

            Type implementationType;
            foreach (var type in abstractInterfaceTypes)
            {
                if (abstractInterfaceImplementations.TryGetValue(type, out implementationType))
                {
                    Type[] genericTypes = { type, implementationType };
                    var genericDefaultFactoryMethod = defaultFactoryMethod.MakeGenericMethod(genericTypes);
                    this.RegisterInternal(type, implementationType, string.Empty, genericDefaultFactoryMethod.Invoke(this, null) as ObjectFactoryBase);
                }
            }
        }

        /// <summary>
        /// Creates/replaces a container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>()
            where RegisterType : class
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterType), string.Empty, GetDefaultObjectFactory<RegisterType, RegisterType>());
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(string name)
            where RegisterType : class
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterType), name, GetDefaultObjectFactory<RegisterType, RegisterType>());
        }

        /// <summary>
        /// Creates/replaces a container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterImplementation), string.Empty, GetDefaultObjectFactory<RegisterType, RegisterImplementation>());
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(string name)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterImplementation), name, GetDefaultObjectFactory<RegisterType, RegisterImplementation>());
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(RegisterType instance)
           where RegisterType : class
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterType), string.Empty, new InstanceFactory<RegisterType, RegisterType>(instance));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterImplementation">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(RegisterType instance, string name)
            where RegisterType : class
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterType), name, new InstanceFactory<RegisterType, RegisterType>(instance));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type of instance to register that implements RegisterType</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(RegisterImplementation instance)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterImplementation), string.Empty, new InstanceFactory<RegisterType, RegisterImplementation>(instance));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type of instance to register that implements RegisterType</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(RegisterImplementation instance, string name)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterImplementation), name, new InstanceFactory<RegisterType, RegisterImplementation>(instance));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(Func<TinyIoCContainer, NamedParameterOverloads, RegisterType> factory)
            where RegisterType : class
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterType), string.Empty, new DelegateFactory<RegisterType>(factory));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(Func<TinyIoCContainer, NamedParameterOverloads, RegisterType> factory, string name)
            where RegisterType : class
        {
            return RegisterInternal(typeof(RegisterType), typeof(RegisterType), name, new DelegateFactory<RegisterType>(factory));
        }

        #endregion

        #region Resolution
        /// <summary>
        /// Attempts to resolve a type using default options.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>()
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType)), NamedParameterOverloads.Default, ResolveOptions.Default) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(ResolveOptions options)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType)), NamedParameterOverloads.Default, options) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType), name), NamedParameterOverloads.Default, ResolveOptions.Default) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a type using supplied options and  name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name, ResolveOptions options)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType), name), NamedParameterOverloads.Default, options) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(NamedParameterOverloads parameters)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType)), parameters, ResolveOptions.Default) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(NamedParameterOverloads parameters, ResolveOptions options)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType)), parameters, options) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters and name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name, NamedParameterOverloads parameters)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType), name), parameters, ResolveOptions.Default) as ResolveType;
        }

        /// <summary>
        /// Attempts to resolve a named type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="TinyIoCResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name, NamedParameterOverloads parameters, ResolveOptions options)
            where ResolveType : class
        {
            return ResolveInternal(new TypeRegistration(typeof(ResolveType), name), parameters, options) as ResolveType;
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>()
            where ResolveType : class
        {
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType)), NamedParameterOverloads.Default, ResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name)
            where ResolveType : class
        {
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType), name), NamedParameterOverloads.Default, ResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(ResolveOptions options)
            where ResolveType : class
        {
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType)), NamedParameterOverloads.Default, options);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name, ResolveOptions options)
            where ResolveType : class
        {
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType), name), NamedParameterOverloads.Default, options);
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
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType)), parameters, ResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters and default options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name, NamedParameterOverloads parameters)
            where ResolveType : class
        {
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType), name), parameters, ResolveOptions.Default);
        }

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
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType)), parameters, options);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name, NamedParameterOverloads parameters, ResolveOptions options)
            where ResolveType : class
        {
            return CanResolveInternal(new TypeRegistration(typeof(ResolveType), name), parameters, options);
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
            /// Constructor to use, if specified
            /// </summary>
            public ConstructorInfo Constructor { get; protected set; }

            /// <summary>
            /// Create the type
            /// </summary>
            /// <param name="container">Container that requested the creation</param>
            /// <param name="parameters">Any user parameters passed</param>
            /// <returns></returns>
            public abstract object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options);

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

            public virtual void SetConstructor(ConstructorInfo constructor)
            {
                this.Constructor = constructor;
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

            public MultiInstanceFactory()
            {
                if (typeof(RegisterImplementation).IsAbstract || typeof(RegisterImplementation).IsInterface)
                    throw new TinyIoCRegistrationTypeExceptiopn(typeof(RegisterImplementation), "MultiInstanceFactory");
            }

            public override object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                try
                {
                    return container.ConstructType(typeof(RegisterImplementation), Constructor, parameters, options);
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
            private Func<TinyIoCContainer, NamedParameterOverloads, RegisterType> _factory;

            public override bool AssumeConstruction { get { return true; } }

            public override Type CreatesType { get { return typeof(RegisterType); } }

            public override object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                try
                {
                    return _factory.Invoke(container, parameters);
                }
                catch (Exception ex)
                {
                    throw new TinyIoCResolutionException(typeof(RegisterType), ex);
                }
            }

            public DelegateFactory(Func<TinyIoCContainer, NamedParameterOverloads, RegisterType> factory)
            {
                if (factory == null)
                    throw new ArgumentNullException("factory");

                _factory = factory;
            }

            public override ObjectFactoryBase WeakReferenceVariant
            {
                get
                {
                    return new WeakDelegateFactory<RegisterType>(_factory);
                }
            }

            public override ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    return this;
                }
            }

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new TinyIoCConstructorResolutionException("Constructor selection is not possible for delegate factory registrations");
            }
        }

        /// <summary>
        /// IObjectFactory that invokes a specified delegate to construct the object
        /// 
        /// Holds the delegate using a weak reference
        /// </summary>
        /// <typeparam name="RegisterType">Registered type to be constructed</typeparam>
        private class WeakDelegateFactory<RegisterType> : ObjectFactoryBase
            where RegisterType : class
        {
            private WeakReference _factory;

            public override bool AssumeConstruction { get { return true; } }

            public override Type CreatesType { get { return typeof(RegisterType); } }

            public override object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                var factory = _factory.Target as Func<TinyIoCContainer, NamedParameterOverloads, RegisterType>;

                if (factory == null)
                    throw new TinyIoCWeakReferenceException(typeof(RegisterType));

                try
                {
                    return factory.Invoke(container, parameters);
                }
                catch (Exception ex)
                {
                    throw new TinyIoCResolutionException(typeof(RegisterType), ex);
                }
            }

            public WeakDelegateFactory(Func<TinyIoCContainer, NamedParameterOverloads, RegisterType> factory)
            {
                if (factory == null)
                    throw new ArgumentNullException("factory");

                _factory = new WeakReference(factory);
            }

            public override ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    var factory = _factory.Target as Func<TinyIoCContainer, NamedParameterOverloads, RegisterType>;

                    if (factory == null)
                        throw new TinyIoCWeakReferenceException(typeof(RegisterType));

                    return new DelegateFactory<RegisterType>(factory);
                }
            }

            public override ObjectFactoryBase WeakReferenceVariant
            {
                get
                {
                    return this;
                }
            }

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new TinyIoCConstructorResolutionException("Constructor selection is not possible for delegate factory registrations");
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
            private RegisterImplementation _instance;

            public InstanceFactory(RegisterImplementation instance)
            {
                this._instance = instance;
            }

            public override Type CreatesType
            {
                get { return typeof(RegisterImplementation); }
            }

            public override object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                return _instance;
            }

            public override ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    return new MultiInstanceFactory<RegisterType, RegisterImplementation>();
                }
            }

            public override ObjectFactoryBase WeakReferenceVariant
            {
                get
                {
                    return new WeakInstanceFactory<RegisterType, RegisterImplementation>(_instance);
                }
            }

            public override ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    return this;
                }
            }

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new TinyIoCConstructorResolutionException("Constructor selection is not possible for instance factory registrations");
            }

            public void Dispose()
            {
                var disposable = _instance as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }
        }

        /// <summary>
        /// Stores an particular instance to return for a type
        /// 
        /// Stores the instance with a weak reference
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Type of the instance</typeparam>
        private class WeakInstanceFactory<RegisterType, RegisterImplementation> : ObjectFactoryBase, IDisposable
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            private WeakReference _instance;

            public WeakInstanceFactory(RegisterImplementation instance)
            {
                this._instance = new WeakReference(instance);
            }

            public override Type CreatesType
            {
                get { return typeof(RegisterImplementation); }
            }

            public override object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                var instance = _instance.Target as RegisterImplementation;

                if (instance == null)
                    throw new TinyIoCWeakReferenceException(typeof(RegisterType));

                return instance;
            }

            public override ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    return new MultiInstanceFactory<RegisterType, RegisterImplementation>();
                }
            }

            public override ObjectFactoryBase WeakReferenceVariant
            {
                get
                {
                    return this;
                }
            }

            public override ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    var instance = _instance.Target as RegisterImplementation;

                    if (instance == null)
                        throw new TinyIoCWeakReferenceException(typeof(RegisterType));

                    return new InstanceFactory<RegisterType, RegisterImplementation>(instance);
                }
            }

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new TinyIoCConstructorResolutionException("Constructor selection is not possible for instance factory registrations");
            }

            public void Dispose()
            {
                var disposable = _instance.Target as IDisposable;

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

            public SingletonFactory()
            {
                if (typeof(RegisterImplementation).IsAbstract || typeof(RegisterImplementation).IsInterface)
                    throw new TinyIoCRegistrationTypeExceptiopn(typeof(RegisterImplementation), "SingletonFactory");
            }

            public override Type CreatesType
            {
                get { return typeof(RegisterImplementation); }
            }

            public override object GetObject(TinyIoCContainer container, NamedParameterOverloads parameters, ResolveOptions options)
            {
                if (parameters.Count != 0)
                    throw new ArgumentException("Cannot specify parameters for singleton types");

                lock (SingletonLock)
                    if (_Current == null)
                        _Current = container.ConstructType(typeof(RegisterImplementation), Constructor, options) as RegisterImplementation;

                return _Current;
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
        private static readonly TinyIoCContainer _Current = new TinyIoCContainer();

        static TinyIoCContainer()
        {
        }

        /// <summary>
        /// Lazy created Singleton instance of the container for simple scenarios
        /// </summary>
        public static TinyIoCContainer Current
        {
            get
            {
                return _Current;
            }
        }
        #endregion

        #region Type Registrations
        public sealed class TypeRegistration
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
        private readonly SafeDictionary<TypeRegistration, ObjectFactoryBase> _RegisteredTypes;
        #endregion

        #region Constructors
        public TinyIoCContainer()
        {
            _RegisteredTypes = new SafeDictionary<TypeRegistration, ObjectFactoryBase>();

            RegisterDefaultTypes();
        }
        #endregion

        #region Internal Methods
        private void RegisterDefaultTypes()
        {
            this.Register<TinyIoCContainer>(this);
        }

        private ObjectFactoryBase GetCurrentFactory(TypeRegistration registration)
        {
            ObjectFactoryBase current = null;

            _RegisteredTypes.TryGetValue(registration, out current);

            return current;
        }

        private RegisterOptions RegisterInternal(Type registerType, Type registerImplementation, string name, ObjectFactoryBase factory)
        {
            var typeRegistration = new TypeRegistration(registerType, name);

            return AddUpdateRegistration(typeRegistration, factory);
        }

        private RegisterOptions AddUpdateRegistration(TypeRegistration typeRegistration, ObjectFactoryBase factory)
        {
            _RegisteredTypes[typeRegistration] = factory;

            return new RegisterOptions(this, typeRegistration);
        }

        private void RemoveRegistration(TypeRegistration typeRegistration)
        {
            _RegisteredTypes.Remove(typeRegistration);
        }

        private ObjectFactoryBase GetDefaultObjectFactory<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            if (typeof(RegisterType).IsInterface || typeof(RegisterType).IsAbstract)
                return new SingletonFactory<RegisterType, RegisterImplementation>();

            return new MultiInstanceFactory<RegisterType, RegisterImplementation>();
        }

        private bool CanResolveInternal(TypeRegistration registration, NamedParameterOverloads parameters, ResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Type checkType = registration.Type;
            string name = registration.Name;

            ObjectFactoryBase factory;
            if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType, name), out factory))
            {
                if (factory.AssumeConstruction)
                    return true;

                if (factory.Constructor == null)
                    return (GetBestConstructor(factory.CreatesType, parameters, options) != null) ? true : false;
                else
                    return CanConstruct(factory.Constructor, parameters, options);
            }

            // Fail if requesting named resolution and settings set to fail if unresolved
            if (!String.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == NamedResolutionFailureActions.Fail)
                return false;

            // Attemped unnamed fallback container resolution if relevant and requested
            if (!String.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == NamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType), out factory))
                {
                    if (factory.AssumeConstruction)
                        return true;

                    return (GetBestConstructor(factory.CreatesType, parameters, options) != null) ? true : false;
                }
            }

            // Check if type is an automatic lazy factory request
            if (IsAutomaticLazyFactoryRequest(checkType))
                return true;

            // Attempt unregistered construction if possible and requested
            if ((options.UnregisteredResolutionAction == UnregisteredResolutionActions.AttemptResolve) || (checkType.IsGenericType && options.UnregisteredResolutionAction == UnregisteredResolutionActions.GenericsOnly))
                return (GetBestConstructor(checkType, parameters, options) != null) ? true : false;

            return false;
        }

        private bool IsAutomaticLazyFactoryRequest(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type genericType = type.GetGenericTypeDefinition();

            // Just a func
            if (genericType == typeof(Func<>))
                return true;

            // 2 parameter func with string as first parameter (name)
            if ((genericType == typeof(Func<,>) && type.GetGenericArguments()[0] == typeof(string)))
                return true;

            return false;
        }

        private object ResolveInternal(TypeRegistration registration, NamedParameterOverloads parameters, ResolveOptions options)
        {
            ObjectFactoryBase factory;

            // Attempt container resolution
            if (_RegisteredTypes.TryGetValue(registration, out factory))
            {
                try
                {
                    return factory.GetObject(this, parameters, options);
                }
                catch (Exception ex)
                {
                    throw new TinyIoCResolutionException(registration.Type, ex);
                }
            }

            // Fail if requesting named resolution and settings set to fail if unresolved
            if (!String.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction == NamedResolutionFailureActions.Fail)
                throw new TinyIoCResolutionException(registration.Type);

            // Attemped unnamed fallback container resolution if relevant and requested
            if (!String.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction == NamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(registration.Type, string.Empty), out factory))
                {
                    try
                    {
                        return factory.GetObject(this, parameters, options);
                    }
                    catch (Exception ex)
                    {
                        throw new TinyIoCResolutionException(registration.Type, ex);
                    }
                }
            }

            // Attempt to construct an automatic lazy factory if possible
            if (IsAutomaticLazyFactoryRequest(registration.Type))
                return GetLazyAutomaticFactoryRequest(registration.Type);

            // Attempt unregistered construction if possible and requested
            if ((options.UnregisteredResolutionAction == UnregisteredResolutionActions.AttemptResolve) || (registration.Type.IsGenericType && options.UnregisteredResolutionAction == UnregisteredResolutionActions.GenericsOnly))
            {
                if (!registration.Type.IsAbstract && !registration.Type.IsInterface)
                    return ConstructType(registration.Type, parameters, options);
            }

            // Unable to resolve - throw
            throw new TinyIoCResolutionException(registration.Type);
        }

        private object GetLazyAutomaticFactoryRequest(Type type)
        {
            if (!type.IsGenericType)
                return null;

            Type genericType = type.GetGenericTypeDefinition();
            Type[] genericArguments = type.GetGenericArguments();

            // Just a func
            if (genericType == typeof(Func<>))
            {
                Type returnType = genericArguments[0];

                MethodInfo resolveMethod = typeof(TinyIoCContainer).GetMethod("Resolve", new Type[] { });
                resolveMethod = resolveMethod.MakeGenericMethod(returnType);

                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod);

                var resolveLambda = Expression.Lambda(resolveCall).Compile();

                return resolveLambda;
            }

            // Named func
            if ((genericType == typeof(Func<,>)) && (genericArguments[0] == typeof(string)))
            {
                Type returnType = genericArguments[1];

                MethodInfo resolveMethod = typeof(TinyIoCContainer).GetMethod("Resolve", new Type[] { typeof(String) });
                resolveMethod = resolveMethod.MakeGenericMethod(returnType);

                ParameterExpression[] resolveParameters = new ParameterExpression[] { Expression.Parameter(typeof(String), "name") };
                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod, resolveParameters);

                var resolveLambda = Expression.Lambda(resolveCall, resolveParameters).Compile();

                return resolveLambda;
            }

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

                if (!parameters.ContainsKey(parameter.Name) && !CanResolveInternal(new TypeRegistration(parameter.ParameterType), NamedParameterOverloads.Default, options))
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

            // Get constructors in reverse order based on the number of parameters
            // i.e. be as "greedy" as possible so we satify the most amount of dependencies possible
            var ctors = from ctor in type.GetConstructors()
                        orderby ctor.GetParameters().Count() descending
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
            return ConstructType(type, null, NamedParameterOverloads.Default, options);
        }

        private object ConstructType(Type type, ConstructorInfo constructor, ResolveOptions options)
        {
            return ConstructType(type, constructor, NamedParameterOverloads.Default, options);
        }

        private object ConstructType(Type type, NamedParameterOverloads parameters, ResolveOptions options)
        {
            return ConstructType(type, null, parameters, options);
        }

        private object ConstructType(Type type, ConstructorInfo constructor, NamedParameterOverloads parameters, ResolveOptions options)
        {
            if (constructor == null)
                constructor = GetBestConstructor(type, parameters, options);

            if (constructor == null)
                throw new TinyIoCResolutionException(type);

            var ctorParams = constructor.GetParameters();
            object[] args = new object[ctorParams.Count()];

            for (int parameterIndex = 0; parameterIndex < ctorParams.Count(); parameterIndex++)
            {
                var currentParam = ctorParams[parameterIndex];

                args[parameterIndex] = parameters.ContainsKey(currentParam.Name) ? parameters[currentParam.Name] : ResolveInternal(new TypeRegistration(currentParam.ParameterType), NamedParameterOverloads.Default, options);
            }

            try
            {
                return constructor.Invoke(args);
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
            _RegisteredTypes.Dispose();
        }

        #endregion
    }
}
