using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;

namespace TinyIoC.Tests.TestData
{
    public class TestMessage : TinyMessageBase
    {
        public TestMessage(object sender) : base(sender)
        {
            
        }
    }

    public class TestProxy : ITinyMessageProxy
    {
        public ITinyMessage Message {get; private set;}

        public void Deliver(ITinyMessage message, ITinyMessageSubscription subscription)
        {
            this.Message = message;
            subscription.Deliver(message);
        }
    }

}
