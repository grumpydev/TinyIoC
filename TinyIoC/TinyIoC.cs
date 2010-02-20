using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC
{
    public class TinyIoC
    {
        private static readonly TinyIoC current = new TinyIoC();

        static TinyIoC()
        {
        }

        private TinyIoC()
        {
        }

        public static TinyIoC Current
        {
            get
            {
                return current;
            }
        }
    }
}
