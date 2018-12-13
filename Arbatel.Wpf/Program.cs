using System;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.WPF_WFControl;
using Arbatel.Controls;
using Arbatel.UI;

namespace Arbatel.Wpf
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var platform = Platform.Detect;

			platform.Add<GLSurface.IHandler>(() => new WPFWFGLSurfaceHandler());

			new Application(platform).Run(new MainForm());
		}
	}
}
