using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyIoC.Tests.TestData;
using TinyMessenger;

#if !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace TinyIoC.Tests
{
    [TestClass]
    public class TinyMessageSubscriptionTokenTests
    {
#if MOQ
        [TestMethod]
        public void Dispose_WithValidHubReference_UnregistersWithHub()
        {
            var messengerMock = new Moq.Mock<ITinyMessengerHub>();
            messengerMock.Setup((messenger) => messenger.Unsubscribe<TestMessage>(Moq.It.IsAny<TinyMessageSubscriptionToken>())).Verifiable();
            var token = new TinyMessageSubscriptionToken(messengerMock.Object, typeof(TestMessage));

            token.Dispose();

            messengerMock.VerifyAll();
        }
#endif

// can't do GC.WaitForFullGCComplete in WinRT...
#if !NETFX_CORE
        [TestMethod]
        public void Dispose_WithInvalidHubReference_DoesNotThrow()
        {
            var token = UtilityMethods.GetTokenWithOutOfScopeMessenger();
            GC.Collect();
            GC.WaitForFullGCComplete(2000);

            token.Dispose();
        }
#endif

        [TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullHub_ThrowsArgumentNullException()
        {
            var messenger = UtilityMethods.GetMessenger();

            AssertHelper.ThrowsException<ArgumentNullException>(() => new TinyMessageSubscriptionToken(null, typeof(ITinyMessage)));
        }

        [TestMethod]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Ctor_InvalidMessageType_ThrowsArgumentOutOfRangeException()
        {
            var messenger = UtilityMethods.GetMessenger();

            AssertHelper.ThrowsException<ArgumentOutOfRangeException>(() => new TinyMessageSubscriptionToken(messenger, typeof(object)));
        }

        [TestMethod]
        public void Ctor_ValidHubAndMessageType_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            var token = new TinyMessageSubscriptionToken(messenger, typeof(TestMessage));
        }
    }
}
