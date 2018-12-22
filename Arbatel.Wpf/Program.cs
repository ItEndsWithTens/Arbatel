using System;
using Arbatel.Controls;
using Arbatel.UI;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.WPF_WFControl;
using OpenTK;

namespace Arbatel.Wpf
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Startup.InitOpenTK();

			var platform = Platform.Detect;

			platform.Add<GLSurface.IHandler>(() => new WPFWFGLSurfaceHandler());

			using (opentk)
			{
				new Application(platform).Run(new MainForm());
			}
		}
	}
}
