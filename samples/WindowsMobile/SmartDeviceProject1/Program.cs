using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartDeviceProject1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            TinyIoC.TinyIoCContainer.Current.AutoRegister();

            Application.Run(new Form1());
        }
    }
}