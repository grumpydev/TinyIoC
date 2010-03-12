#define TINYMESSENGER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyMessenger
{
    public interface ITinyMessage
    {
        object Sender { get; }
    }

    public abstract class TinyMessageBase : ITinyMessage
    {
        /// <summary>
        /// Store a WeakReference to the sender just in case anyone is daft enough to
        /// keep the message around and prevent the sender from being collected.
        /// </summary>
        private WeakReference _Sender;
        public object Sender
        {
            get 
            { 
                return (_Sender == null) ? null : _Sender.Target; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the MessageBase class.
        /// </summary>
        /// <param name="sender">Message sender (usually "this")</param>
        public TinyMessageBase(object sender)
        {
            _Sender = new WeakReference(sender);
        }
    }

    public interface ITinyMessengerHub
    {
        void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction) where TMessage : class, ITinyMessage;
        void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, ITinyMessage;
    }

    #region Exceptions
    public class TinyMessengerSubscriptionException : Exception
    {
        private const string ERROR_TEXT = "Unable to add subscription for {0} : {1}";

        public TinyMessengerSubscriptionException(Type messageType, string reason)
            : base(String.Format(ERROR_TEXT, messageType, reason))
        {
            
        }

        public TinyMessengerSubscriptionException(Type messageType, string reason, Exception innerException)
            : base(String.Format(ERROR_TEXT, messageType, reason), innerException)
        {

        }
    }
    #endregion

    public sealed class TinyMessengerHub : ITinyMessengerHub
    {
        #region Private Types and Interfaces
        private interface ITinyMessageSubscription
        {
            object Destination { get; }
            bool DestinationActive { get; }
            bool ShouldSend(ITinyMessage message);
            void Send(ITinyMessage message);
        }

        private class TinyMessageSubscription<TMessage> : ITinyMessageSubscription
            where TMessage : class, ITinyMessage
        {
            private WeakReference _Destination;
            private Action<TMessage> _DeliveryAction;
            private Func<TMessage, bool> _MessageFilter;

            public object Destination
            {
                get { return _Destination.Target; }
            }

            public bool DestinationActive
            {
                get { return _Destination.IsAlive; }
            }

            public bool ShouldSend(ITinyMessage message)
            {
                if (!(message is TMessage))
                    return false;

                return _MessageFilter.Invoke(message as TMessage);
            }

            public void Send(ITinyMessage message)
            {
                if (!(message is TMessage))
                    throw new ArgumentException("Message is not the correct type");

                _DeliveryAction.Invoke(message as TMessage);
            }

            /// <summary>
            /// Initializes a new instance of the TinyMessageSubscription class.
            /// </summary>
            /// <param name="destination">Destination object</param>
            /// <param name="deliveryAction">Delivery action</param>
            /// <param name="messageFilter">Filter function</param>
            public TinyMessageSubscription(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
            {
                _Destination = new WeakReference(destination);
                _DeliveryAction = deliveryAction;
                _MessageFilter = messageFilter;
            }
        }
        #endregion

        #region Subscription dictionary
        private readonly object _SubscriptionsPadlock = new object();
        private readonly Dictionary<Type, List<ITinyMessageSubscription>> _Subscriptions = new Dictionary<Type, List<ITinyMessageSubscription>>();
        #endregion

        #region Public API
        public void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction) where TMessage : class, ITinyMessage
        {
            Subscribe<TMessage>(destination, deliveryAction, (m) => true);
        }

        public void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, ITinyMessage
        {
            AddSubscriptionInternal<TMessage>(destination, deliveryAction, messageFilter);
        }
        #endregion

        #region Internal Methods
        private void AddSubscriptionInternal<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
                where TMessage : class, ITinyMessage
        {
            lock (_SubscriptionsPadlock)
            {
                List<ITinyMessageSubscription> currentSubscriptions;

                if (!_Subscriptions.TryGetValue(typeof(TMessage), out currentSubscriptions))
                {
                    currentSubscriptions = new List<ITinyMessageSubscription>();
                    _Subscriptions[typeof(TMessage)] = currentSubscriptions;
                }

                var currentlySubscribed = from sub in currentSubscriptions
                                          where (sub.Destination != null) && (object.ReferenceEquals(sub.Destination, destination))
                                          select sub;

                if (currentlySubscribed.Count() != 0)
                    throw new TinyMessengerSubscriptionException(typeof(TMessage), "An existing subscription for that message type and destination already exists");

                currentSubscriptions.Add(new TinyMessageSubscription<TMessage>(destination, deliveryAction, messageFilter));
            }
        }
        #endregion
    }
}
