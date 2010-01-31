using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Ohm.FarmVille {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			try {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FarmVilleTrainer());
			} catch (Exception ex) {
				MessageBox.Show(ex.StackTrace);
			}
		}
	}
}