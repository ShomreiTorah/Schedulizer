using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using System.IO;

namespace ShomreiTorah.Schedules.WinClient {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			BonusSkins.Register();
			if (Path.GetFileName(Path.GetDirectoryName(typeof(Program).Assembly.Location)) == "Debug")
				UserLookAndFeel.Default.SkinName = "Office 2007 Black";
			else
				UserLookAndFeel.Default.SkinName = "Office 2007 Blue";

			Application.Run(new MainForm());
		}
	}
}
