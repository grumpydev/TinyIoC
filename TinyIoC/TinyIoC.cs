using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public class TinyIoC
    {
        #region Object Factories
        private interface IObjectFactory
        {
            object GetObject();
        }

        private class NewInstanceFactory<RegisterType, RegisterImplementation> : IObjectFactory
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            public object GetObject()
            {
                try
                {
                    return Activator.CreateInstance(typeof(RegisterImplementation));
                }
                catch (System.MissingMemberException ex)
                {
                    throw new TinyIoCResolutionException(typeof(RegisterImplementation), ex);
                }
            }
        }

        private class DelegateFactory<RegisterType> : IObjectFactory
        {
            private Func<TinyIoC, RegisterType> _Factory;

            public DelegateFactory(Func<TinyIoC, RegisterType> factory)
            {
                if (factory == null)
                    throw new ArgumentNullException("factory");

                _Factory = factory;
            }

            public object GetObject()
            {
                return _Factory.Invoke(_Current);
            }
        }

        private class SingletonFactory<RegisterType, RegisterImplementation> : IObjectFactory
        {
            private static readonly RegisterType _Current;

            static SingletonFactory()
            {
                // TODO - Instantiate _Current
            }

            public object GetObject()
            {
                return _Current;
            }
        }
        #endregion

        public sealed class RegisterOptions<RegisterType, RegisterImplementation>
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
        }

        private static readonly TinyIoC _Current = new TinyIoC();

        static TinyIoC()
        {
        }

        private TinyIoC()
        {
            _RegisteredTypes = new Dictionary<Type, IObjectFactory>();
        }

        public static TinyIoC Current
        {
            get
            {
                return _Current;
            }
        }

        private readonly Dictionary<Type, IObjectFactory> _RegisteredTypes;

        #region Static Methods
        public static RegisterOptions<RegisterImplementation, RegisterImplementation> Register<RegisterImplementation>()
            where RegisterImplementation : class
        {
            return _Current.RegisterPrivate<RegisterImplementation>();
        }

        public static RegisterOptions<RegisterType, RegisterImplementation> Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return _Current.RegisterPrivate<RegisterType, RegisterImplementation>();
        }

        public static void Register<RegisterType>(Func<TinyIoC, RegisterType> factory)
        {
            _Current.RegisterPrivate<RegisterType>(factory);
        }

        public static RegisterType Resolve<RegisterType>()
            where RegisterType : class
        {
            return _Current.ResolvePrivate<RegisterType>();
        }

        private static IObjectFactory GetDefaultObjectFactory<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            // TODO : Add logic
            return new NewInstanceFactory<RegisterType, RegisterImplementation>();
        }
        #endregion

        #region Instance Methods
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

        public void RegisterPrivate<RegisterType>(Func<TinyIoC, RegisterType> factory)
        {
            _RegisteredTypes[typeof(RegisterType)] = new DelegateFactory<RegisterType>(factory);
        }

        public RegisterType ResolvePrivate<RegisterType>()
            where RegisterType : class
        {
            IObjectFactory factory;

            if (_RegisteredTypes.TryGetValue(typeof(RegisterType), out factory))
            {
                return factory.GetObject() as RegisterType;
            }
            else
            {
                throw new TinyIoCResolutionException(typeof(RegisterType));
            }
        }
        #endregion

    }
}
