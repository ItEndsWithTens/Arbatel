using Arbatel.UI;
using Eto;
using Eto.Forms;
using OpenTK;
using System;

namespace Arbatel.Wpf
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

			Platform platform = Platform.Detect;

			var application = new Application(platform);
			application.UnhandledException += Core.UnhandledExceptionHandler;

			using (opentk)
			{
				application.Run(new MainForm());
			}
		}
	}
}
