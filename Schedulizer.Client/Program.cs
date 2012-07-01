using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using System.IO;
using ShomreiTorah.Common;

namespace ShomreiTorah.Schedules.WinClient {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Config.ForceLoad();
			if (Config.IsDebug)
				UserLookAndFeel.Default.SkinName = "DevExpress Dark Style";
			else
				UserLookAndFeel.Default.SkinName = "Office 2010 Blue";

			Application.Run(new MainForm());
		}
	}
}
