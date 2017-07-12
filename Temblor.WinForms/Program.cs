using System;
using Eto;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Windows;

namespace Temblor.WinForms
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var platform = Platform.Detect;

			platform.Add<GLSurface.IHandler>(() => new WinGLSurfaceHandler());

			new Application(platform).Run(new MainForm());
		}
	}
}
