using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyIoC.Tests.PlatformTestSuite;

namespace SilverlightPlatformTests
{
    public class ListLogger : ILogger
    {
        public List<String> LogEntries { get; private set; }

        public ListLogger()
        {
            LogEntries = new List<string>();
        }

        #region ILogger Members

        public void WriteLine(string text)
        {
            LogEntries.Add(text);
        }

        #endregion
    }

    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            var logger = new ListLogger();
            var tests = new PlatformTests(logger);
            int testsRun;
            int testsPassed;
            int testsFailed;
            tests.RunTests(out testsRun, out testsPassed, out testsFailed);
            Results.Items.Clear();
            foreach (var item in logger.LogEntries)
            {
                Results.Items.Add(item);
            }
            MessageBox.Show(String.Format("{0} tests run. {1} passed, {2} failed.", testsRun, testsPassed, testsFailed));
        }
    }
}
