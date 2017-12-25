using System;
using Eto;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Gtk;

namespace Temblor.Gtk2
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var platform = new Eto.GtkSharp.Platform();

			platform.Add<GLSurface.IHandler>(() => new GtkGlSurfaceHandler());

			new Application(platform).Run(new MainForm());
		}
	}
}
