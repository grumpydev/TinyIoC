using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyIoC.Tests.TestData;
using TinyMessenger;

namespace TinyIoC.Tests
{
    [TestClass]
    public class TinyMessengerTests
    {
        [TestMethod]
        public void TinyMessenger_Ctor_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
        }

        [TestMethod]
        public void Subscribe_ValidDestinationAndDeliverAction_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>));
        }

        [TestMethod]
        public void Subscribe_ValidDestinationAndDeliverActionWIthStrongReferences_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), true);
        }

        [TestMethod]
        public void Subscribe_ValidDestinationDeliveryActionAndFilter_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_NullDestination_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(null, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_NullDeliveryAction_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, null, new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_NullFilter_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), null);
        }

        [TestMethod]
        [ExpectedException(typeof(TinyMessengerSubscriptionException))]
        public void Subscribe_SameDestinationAndEventTwice_ThrowsException()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        public void Unsubscribe_NoPreviousSubscription_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Unsubscribe<TestMessage>(this);
        }

        [TestMethod]
        public void Unsubscribe_PreviousSubscription_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));

            messenger.Unsubscribe<TestMessage>(this);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Unsubscribe_NullDestination_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Unsubscribe<TestMessage>(null);
        }

        [TestMethod]
        public void Unsubscribe_PreviousSubscription_CanSubscribeAgainWithoutThrowing()
        {
            var messenger = UtilityMethods.GetMessenger();
            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
            messenger.Unsubscribe<TestMessage>(this);

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Publish_NullMessage_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Publish<TestMessage>(null);
        }

        [TestMethod]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Publish<TestMessage>(new TestMessage(this));
        }

        [TestMethod]
        public void Publish_Subscriber_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));

            messenger.Publish<TestMessage>(new TestMessage(this));
        }

        [TestMethod]
        public void Publish_SubscribedMessageNoFilter_GetsMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            bool received = false;
            messenger.Subscribe<TestMessage>(this, (m) => { received = true; });

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsTrue(received);
        }

        [TestMethod]
        public void Publish_SubscribedMessageButFiltered_DoesNotGetMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            bool received = false;
            messenger.Subscribe<TestMessage>(this, (m) => { received = true; }, (m) => false);

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsFalse(received);
        }

        [TestMethod]
        public void Publish_SubscribedMessageNoFilter_GetsActualMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            ITinyMessage receivedMessage = null;
            var payload = new TestMessage(this);
            messenger.Subscribe<TestMessage>(this, (m) => { receivedMessage = m; });

            messenger.Publish<TestMessage>(payload);

            Assert.ReferenceEquals(payload, receivedMessage);
        }

    }
}