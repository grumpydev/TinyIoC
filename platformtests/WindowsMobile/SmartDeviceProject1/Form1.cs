using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TinyIoC.Tests.PlatformTestSuite;

namespace SmartDeviceProject1
{
    public partial class Form1 : Form
    {
        public class ListBoxLogger : ILogger
        {
            ListBox _ListBox;

            public ListBoxLogger(ListBox listBox)
            {
                _ListBox = listBox;
            }

            public void WriteLine(string text)
            {
                _ListBox.Items.Add(text);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void pushMeButton_Click(object sender, EventArgs e)
        {
            var tests = new PlatformTests(new ListBoxLogger(listBox1));
            int run;
            int passed;
            int failed;

            tests.RunTests(out run, out passed, out failed);

            MessageBox.Show("Run: " + run + " Passed: " + passed + " Failed: " + failed);
        }
    }
}