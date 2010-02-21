using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC
{
    #region Exceptions
    public class TinyIoCResolutionException : Exception
    {
        public TinyIoCResolutionException(string message)
            : base(message)
        {
        }

        public TinyIoCResolutionException(string message, Exception innerException)
            : base(message, innerException)
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
                    throw new TinyIoCResolutionException(String.Format("Unable to construct type: {0}", typeof(RegisterImplementation)), ex);
                }
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

        public sealed class RegisterOptions
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

        private object _TypeLock = new object();
        private readonly Dictionary<Type, IObjectFactory> _RegisteredTypes;

        #region Static Methods
        public static RegisterOptions Register<RegisterImplementation>()
            where RegisterImplementation : class
        {
            return _Current.RegisterPrivate<RegisterImplementation>();
        }

        public static RegisterOptions Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return _Current.RegisterPrivate<RegisterType, RegisterImplementation>();
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
        private RegisterOptions RegisterPrivate<RegisterImplementation>()
            where RegisterImplementation : class
        {
            return RegisterPrivate<RegisterImplementation, RegisterImplementation>();
        }

        private RegisterOptions RegisterPrivate<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
