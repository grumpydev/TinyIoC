using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SmartDeviceProject1
{
    public class TestClass : ITestInterface
    {
        #region ITestInterface Members

        public string GetMessage()
        {
            return "Hello from TinyIoC!";
        }

        #endregion
    }
}
