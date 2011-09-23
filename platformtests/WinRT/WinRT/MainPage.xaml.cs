using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyIoC.Tests.PlatformTestSuite;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace WinRT
{
    partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            var logger = new StringLogger();
            var tests = new PlatformTests(logger);
            int testsRun, testsPassed, testsFailed;
            tests.RunTests(out testsRun, out testsPassed, out testsFailed);
            Results.Text = String.Format("{0} Run, {1} Passed, {2} Failed", testsRun, testsPassed, testsFailed);
            ResultsBox.Text = logger.Log;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
