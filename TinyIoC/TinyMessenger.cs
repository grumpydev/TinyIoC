//===============================================================================
// TinyIoC - TinyMessenger
//
// A simple messenger/event aggregator.
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

namespace TinyMessenger
{
    #region Message Types / Interfaces
    /// <summary>
    /// A TinyMessage to be published/delivered by TinyMessenger
    /// </summary>
    public interface ITinyMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        object Sender { get; }
    }

    /// <summary>
    /// Base class for messages that provides weak refrence storage of the sender
    /// </summary>
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
            if (sender == null)
                throw new ArgumentNullException("sender");

            _Sender = new WeakReference(sender);
        }
    }

    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content to store</typeparam>
    public class GenericTinyMessage<TContent> : TinyMessageBase
    {
        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent _Content { get; protected set; }

        /// <summary>
        /// Create a new instance of the GenericTinyMessage class.
        /// </summary>
        /// <param name="sender">Message sender (usually "this")</param>
        /// <param name="content">Contents of the message</param>
        public GenericTinyMessage(object sender, TContent content)
            : base(sender)
        {
            _Content = content;
        }
    }
    #endregion

    #region Exceptions
    /// <summary>
    /// Thrown when an exceptions occurs while subscribing to a message type
    /// </summary>
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

    #region Hub Interface
    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public interface ITinyMessengerHub
    {
        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// All references are held with WeakReferences
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception> 
        void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction) where TMessage : class, ITinyMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception> 
        void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, bool useStrongReferences) where TMessage : class, ITinyMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception>          
        void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, ITinyMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - held in a weak reference used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception>          
        void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences) where TMessage : class, ITinyMessage;

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") that was used to subscribe initially</param>
        void Unsubscribe<TMessage>(object destination) where TMessage : class, ITinyMessage;

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        void Publish<TMessage>(TMessage message) where TMessage : class, ITinyMessage;
    }
    #endregion

    #region Hub Implementation
    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public sealed class TinyMessengerHub : ITinyMessengerHub
    {
        #region Private Types and Interfaces
        // Horrible fudge to use as the dictionary data type
        // can't think of any other way without generic contravariance?
        private interface ITinyMessageSubscription
        {
            object Destination { get; }
            bool CanDeliver(ITinyMessage message);
            void Deliver(ITinyMessage message);
        }

        private class WeakTinyMessageSubscription<TMessage> : ITinyMessageSubscription
            where TMessage : class, ITinyMessage
        {
            protected WeakReference _Destination;
            protected WeakReference _DeliveryAction;
            protected WeakReference _MessageFilter;

            public object Destination
            {
                get { return _Destination.Target; }
            }

            public bool CanDeliver(ITinyMessage message)
            {
                if (!(message is TMessage))
                    return false;

                if (!_Destination.IsAlive)
                    return false;

                if (!_DeliveryAction.IsAlive)
                    return false;

                if (!_MessageFilter.IsAlive)
                    return false;

                return ((Func<TMessage, bool>) _MessageFilter.Target).Invoke(message as TMessage);
            }

            public void Deliver(ITinyMessage message)
            {
                if (!(message is TMessage))
                    throw new ArgumentException("Message is not the correct type");

                if (!_Destination.IsAlive)
                    return;

                if (!_DeliveryAction.IsAlive)
                    return;

                try
                {
                    ((Action<TMessage>) _DeliveryAction.Target).Invoke(message as TMessage);
                }
                catch (Exception)
                {
                    // We don't want publish exceptions to bubble up
                }
            }

            /// <summary>
            /// Initializes a new instance of the WeakTinyMessageSubscription class.
            /// </summary>
            /// <param name="destination">Destination object</param>
            /// <param name="deliveryAction">Delivery action</param>
            /// <param name="messageFilter">Filter function</param>
            public WeakTinyMessageSubscription(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
            {
                if (destination == null)
                    throw new ArgumentNullException("destination");

                if (deliveryAction == null)
                    throw new ArgumentNullException("deliveryAction");

                if (messageFilter == null)
                    throw new ArgumentNullException("messageFilter");

                _Destination = new WeakReference(destination);
                _DeliveryAction = new WeakReference(deliveryAction);
                _MessageFilter = new WeakReference(messageFilter);
            }
        }

        private class StrongTinyMessageSubscription<TMessage> : ITinyMessageSubscription
            where TMessage : class, ITinyMessage
        {
            protected object _Destination;
            protected Action<TMessage> _DeliveryAction;
            protected Func<TMessage, bool> _MessageFilter;

            public object Destination
            {
                get { return _Destination; }
            }

            public bool CanDeliver(ITinyMessage message)
            {
                if (!(message is TMessage))
                    return false;

                return _MessageFilter.Invoke(message as TMessage);
            }

            public void Deliver(ITinyMessage message)
            {
                if (!(message is TMessage))
                    throw new ArgumentException("Message is not the correct type");

                try
                {
                    _DeliveryAction.Invoke(message as TMessage);
                }
                catch (Exception)
                {
                    // We don't want publish exceptions to bubble up
                }
            }

            /// <summary>
            /// Initializes a new instance of the TinyMessageSubscription class.
            /// </summary>
            /// <param name="destination">Destination object</param>
            /// <param name="deliveryAction">Delivery action</param>
            /// <param name="messageFilter">Filter function</param>
            public StrongTinyMessageSubscription(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
            {
                if (destination == null)
                    throw new ArgumentNullException("destination");

                if (deliveryAction == null)
                    throw new ArgumentNullException("deliveryAction");

                if (messageFilter == null)
                    throw new ArgumentNullException("messageFilter");

                _Destination = destination;
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
        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// All references are held with WeakReferences
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception> 
        public void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction) where TMessage : class, ITinyMessage
        {
            AddSubscriptionInternal<TMessage>(destination, deliveryAction, (m) => true, false);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception> 
        public void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, bool useStrongReferences) where TMessage : class, ITinyMessage
        {
            AddSubscriptionInternal<TMessage>(destination, deliveryAction, (m) => true, useStrongReferences);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception>          
        public void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, ITinyMessage
        {
            AddSubscriptionInternal<TMessage>(destination, deliveryAction, messageFilter, false);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// 
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") - held in a weak reference used for Unsubscribe <see cref="Unsubscribe"/></param>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <exception cref="TinyMessengerSubscriptionException">Thrown when attempting to subscribe more than once with the same destination and message type</exception>          
        public void Subscribe<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences) where TMessage : class, ITinyMessage
        {
            AddSubscriptionInternal<TMessage>(destination, deliveryAction, messageFilter, useStrongReferences);
        }

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="destination">Destination (usually "this") that was used to subscribe initially</param>
        public void Unsubscribe<TMessage>(object destination) where TMessage : class, ITinyMessage
        {
            RemoveSubscriptionInternal<TMessage>(destination);
        }

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public void Publish<TMessage>(TMessage message) where TMessage : class, ITinyMessage
        {
            PublishInternal<TMessage>(message);
        }
        #endregion

        #region Internal Methods
        private void AddSubscriptionInternal<TMessage>(object destination, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool strongReference)
                where TMessage : class, ITinyMessage
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            if (deliveryAction == null)
                throw new ArgumentNullException("deliveryAction");

            if (messageFilter == null)
                throw new ArgumentNullException("messageFilter");

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

                if (strongReference)
                    currentSubscriptions.Add(new StrongTinyMessageSubscription<TMessage>(destination, deliveryAction, messageFilter));
                else
                    currentSubscriptions.Add(new WeakTinyMessageSubscription<TMessage>(destination, deliveryAction, messageFilter));
            }
        }

        private void RemoveSubscriptionInternal<TMessage>(object destination)
                where TMessage : class, ITinyMessage
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            lock (_SubscriptionsPadlock)
            {
                List<ITinyMessageSubscription> currentSubscriptions;
                if (!_Subscriptions.TryGetValue(typeof(TMessage), out currentSubscriptions))
                    return;

                var currentlySubscribed = (from sub in currentSubscriptions
                                           where (sub.Destination != null) && (object.ReferenceEquals(sub.Destination, destination))
                                           select sub).ToList();

                currentlySubscribed.ForEach(sub => currentSubscriptions.Remove(sub));
            }
        }

        private void PublishInternal<TMessage>(TMessage message)
                where TMessage : class, ITinyMessage
        {
            if (message == null)
                throw new ArgumentNullException("message");

            List<ITinyMessageSubscription> currentlySubscribed;
            lock (_SubscriptionsPadlock)
            {
                List<ITinyMessageSubscription> currentSubscriptions;
                if (!_Subscriptions.TryGetValue(typeof(TMessage), out currentSubscriptions))
                    return;

                currentlySubscribed = (from sub in currentSubscriptions
                                       where (sub.Destination != null) && (sub.CanDeliver(message))
                                       select sub).ToList();
            }

            foreach (var sub in currentlySubscribed)
            {
                sub.Deliver(message);
            }
        }
        #endregion
    }
    #endregion
}
