using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC.Tests.TestData
{
    public class UtilityMethods
    {
        internal static TinyIoC GetContainer()
        {
            return new TinyIoC();
        }
    }
}
