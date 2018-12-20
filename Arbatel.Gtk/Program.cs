using System;
using Eto;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Gtk;
using Arbatel.UI;
using Arbatel.Controls;

namespace Arbatel.Gtk
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var platform = new Eto.GtkSharp.Platform();

			platform.Add<GLSurface.IHandler>(() => new GtkGlSurfaceHandler());

			Style.Add<View>(
				"hidecursor",
				handler =>
				{
					Gdk.Window window = handler.ToNative().GdkWindow;

					var pixmap = new Gdk.Pixmap(null, 1, 1, 1);
					var cursor = new Gdk.Cursor(pixmap, pixmap, Gdk.Color.Zero, Gdk.Color.Zero, 0, 0);

					var mask = Gdk.EventMask.PointerMotionMask | Gdk.EventMask.ButtonPressMask;

					// Doesn't successfully limit mouse motion when running in
					// Windows, but that's an edge case anyway, so no big deal.
					Gdk.Pointer.Grab(window, true, mask, window, cursor, 0);
				});

			Style.Add<View>(
				"showcursor",
				handler =>
				{
					Gdk.Pointer.Ungrab(0);
				});

			new Application(platform).Run(new MainForm());
		}
	}
}
