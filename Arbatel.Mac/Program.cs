using System;
using Eto;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Mac;
using Arbatel.UI;

namespace Arbatel.Mac
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var platform = Platform.Detect;

			platform.Add<GLSurface.IHandler>(() => new MacGLSurfaceHandler());

			new Application(platform).Run(new MainForm());
		}
	}
}
