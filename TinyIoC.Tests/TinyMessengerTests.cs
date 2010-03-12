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
        public void Subscribe_ValidDestinationDeliveryActionAndFilter_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(TinyMessengerSubscriptionException))]
        public void Subscribe_SameDestinationAndEventTwice_ThrowsException()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
            messenger.Subscribe<TestMessage>(this, new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }
    }
}
