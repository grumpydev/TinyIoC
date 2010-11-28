using System;
using TinyIoC.Tests.PlatformTestSuite;
using System.Collections.Generic;
using MonoTouch.UIKit;
namespace MonoTouchTinyIoCPlatformTests
{
	public class ListLogger : ILogger
	{
		public List<String> LogEntries { get; private set; }

		public ListLogger ()
		{
			LogEntries = new List<string> ();
		}

		#region ILogger Members

		public void WriteLine (string text)
		{
			LogEntries.Add (text);
		}
		
		#endregion
	}

	public partial class AppDelegate
	{
		partial void buttonClick (MonoTouch.UIKit.UIButton sender)
		{
			var logger = new ListLogger ();
			var platformTests = new TinyIoC.Tests.PlatformTestSuite.PlatformTests (logger);
			int run;
			int passed;
			int failed;
			platformTests.RunTests (out run, out passed, out failed);
			this.results.Text = "";
			foreach (var item in logger.LogEntries) {
				this.results.Text += item + "\n";
			}
			using (var alert = new UIAlertView ("TinyIoC", String.Format ("{0} tests run. {1} passed, {2} failed.", run, passed, failed), null, "OK", null)) {
				alert.Show ();
			}

		}
	}
}

