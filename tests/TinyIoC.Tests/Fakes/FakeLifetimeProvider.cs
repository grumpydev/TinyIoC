using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC.Tests.Fakes
{
    public class FakeLifetimeProvider : TinyIoC.TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        public object TheObject { get; set; }

        public object GetObject()
        {
            return TheObject;
        }

        public void SetObject(object value)
        {
            TheObject = value;
        }

        public void ReleaseObject()
        {
            TheObject = null;
        }
    }
}
