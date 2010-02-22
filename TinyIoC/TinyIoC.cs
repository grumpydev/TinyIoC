using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TinyIoC
{
    #region Exceptions
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
    #endregion

    public sealed class TinyIoC
    {
        #region Object Factories
        public interface IObjectFactory
        {
            /// <summary>
            /// Whether to assume this factory sucessfully constructs its objects
            /// 
            /// Generally set to true for delegate style factories as CanResolve cannot delve
            /// into the delegates they contain.
            /// </summary>
            bool AssumeConstruction { get; }

            /// <summary>
            /// The type the factory instantiates
            /// </summary>
            Type CreatesType { get; }

            /// <summary>
            /// Create the type
            /// </summary>
            /// <param name="container">Container that requested the creation</param>
            /// <param name="parameters">Any user parameters passed</param>
            /// <returns></returns>
            object GetObject(TinyIoC container, NamedParameterOverloads parameters);
        }

        /// <summary>
        /// IObjectFactory that creates new instances of types for each resolution
        /// </summary>
        /// <typeparam name="RegisterType">Registered type</typeparam>
        /// <typeparam name="RegisterImplementation">Type to construct to fullful request for RegisteredType</typeparam>
        private class NewInstanceFactory<RegisterType, RegisterImplementation> : IObjectFactory
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            public bool AssumeConstruction { get { return false; } }

            public Type CreatesType { get { return typeof(RegisterImplementation); } }

            public object GetObject(NamedParameterOverloads parameters)
            {
                try
                {
                    return TinyIoC.Current.ConstructPrivate(typeof(RegisterImplementation), parameters);
                }
                catch (TinyIoCResolutionException ex)
                {
                    throw new TinyIoCResolutionException(typeof(RegisterImplementation), ex);
                }
            }
        }

        /// <summary>
        /// IObjectFactory that invokes a specified delegate to construct the object
        /// </summary>
        /// <typeparam name="RegisterType">Registered type to be constructed</typeparam>
        private class DelegateFactory<RegisterType> : IObjectFactory
        {
            // TODO : Weak reference?
            private Func<TinyIoC, NamedParameterOverloads, RegisterType> _Factory;
            private TinyIoC _Container;

            public DelegateFactory(Func<TinyIoC, NamedParameterOverloads, RegisterType> factory, TinyIoC container)
            {
                if (factory == null)
                    throw new ArgumentNullException("factory");

                _Factory = factory;
                _Container = container;
            }

            public bool AssumeConstruction { get { return true; } }

            public Type CreatesType { get { return typeof(RegisterType); } }

            public object GetObject(NamedParameterOverloads parameters)
            {
                return _Factory.Invoke(_Container, parameters);
            }
        }

        //private class SingletonFactory<RegisterType, RegisterImplementation> : IObjectFactory
        //{
        //    private static readonly RegisterType _Current;

        //    static SingletonFactory()
        //    {
        //        // TODO - Instantiate _Current
        //    }

        //    public bool AssumeConstruction
        //    {
        //        get
        //        {
        //            return false;
        //        }
        //    }

        //    public Type CreatesType
        //    {
        //        get { return typeof(RegisterImplementation); }
        //    }

        //    public object GetObject(NamedParameterOverloads parameters)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
        #endregion
        #region Setup / Settings Classes
        /// <summary>
        /// Name/Value pairs for specifying "user" parameters when resolving
        /// </summary>
        public sealed class NamedParameterOverloads : Dictionary<string, object>
        {
        }

        /// <summary>
        /// Resolution settings
        /// </summary>
        public sealed class ResolutionOptions
        {
            /// <summary>
            /// Whether to attempt to resolve unregistered types
            /// </summary>
            public bool IncludeUnregistered { get; set; }

            public ResolutionOptions()
                : this(true)
            {
            }

            public ResolutionOptions(bool includeUnregistered)
            {
                IncludeUnregistered = includeUnregistered;
            }

            public static ResolutionOptions GetDefault()
            {
                return new ResolutionOptions();
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
            // TODO - Implement
        }
        #endregion
        private static readonly TinyIoC _Current = new TinyIoC();

        static TinyIoC()
        {
        }

        public TinyIoC()
        {
            _RegisteredTypes = new Dictionary<Type, IObjectFactory>();

            RegisterDefaultTypes();
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

        // TODO - Replace Type with custom class to allow for named resolution?
        private readonly Dictionary<Type, IObjectFactory> _RegisteredTypes;

        #region Utility Methods
        private static IObjectFactory GetDefaultObjectFactory<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            // TODO : Add logic
            return new NewInstanceFactory<RegisterType, RegisterImplementation>();
        }

        private void RegisterDefaultTypes()
        {
            // TODO - register the container and other classes if required
        }
        #endregion


        private void ResetTypesPrivate()
        {
            _RegisteredTypes.Clear();
        }

        private RegisterOptions<RegisterImplementation, RegisterImplementation> RegisterPrivate<RegisterImplementation>()
            where RegisterImplementation : class
        {
            return RegisterPrivate<RegisterImplementation, RegisterImplementation>();
        }

        private RegisterOptions<RegisterType, RegisterImplementation> RegisterPrivate<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            // TODO : Do something if type already exists
            _RegisteredTypes[typeof(RegisterType)] = GetDefaultObjectFactory<RegisterType, RegisterImplementation>();
            return new RegisterOptions<RegisterType, RegisterImplementation>();
        }

        private void RegisterPrivate<RegisterType>(Func<NamedParameterOverloads, RegisterType> factory)
        {
            _RegisteredTypes[typeof(RegisterType)] = new DelegateFactory<RegisterType>(factory, this);
        }

        private RegisterType ResolvePrivate<RegisterType>()
            where RegisterType : class
        {
            return ResolvePrivate<RegisterType>(new NamedParameterOverloads());
        }

        private RegisterType ResolvePrivate<RegisterType>(NamedParameterOverloads parameters)
            where RegisterType : class
        {
            IObjectFactory factory;

            if (_RegisteredTypes.TryGetValue(typeof(RegisterType), out factory))
            {
                return factory.GetObject(parameters) as RegisterType;
            }
            else
            {
                // TODO - use main object creation to create
                if (typeof(RegisterType).IsAbstract || typeof(RegisterType).IsInterface)
                    throw new TinyIoCResolutionException(typeof(RegisterType));
                else
                {
                    try
                    {
                        return ConstructPrivate(typeof(RegisterType), parameters) as RegisterType;
                    }
                    catch (TinyIoCResolutionException ex)
                    {
                        throw new TinyIoCResolutionException(typeof(RegisterType), ex);
                    }
                }

            }
        }

        private bool CanResolvePrivate(Type type)
        {
            return CanResolvePrivate(type, new NamedParameterOverloads());
        }

        private bool CanResolvePrivate(Type type, NamedParameterOverloads parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Type checkType = type;

            IObjectFactory factory;
            if (_RegisteredTypes.TryGetValue(checkType, out factory))
            {
                if (factory.AssumeConstruction)
                    return true;

                checkType = factory.CreatesType;
            }

            var ctors = from ctor in checkType.GetConstructors()
                        orderby ctor.GetParameters().Count()
                        select ctor;

            foreach (var ctor in ctors)
            {
                if (CanConstruct(ctor, parameters))
                    return true;
            }

            return false;
        }

        private bool CanConstruct(ConstructorInfo ctor, NamedParameterOverloads parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            foreach (var parameter in ctor.GetParameters())
            {
                if (!parameters.ContainsKey(parameter.Name) && !CanResolvePrivate(parameter.ParameterType))
                    return false;
            }

            return true;
        }

        public object ConstructPrivate(Type type, NamedParameterOverloads parameters)
        {
            throw new NotImplementedException();
        }

    }
}
