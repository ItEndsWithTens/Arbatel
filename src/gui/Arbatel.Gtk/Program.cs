using Arbatel.Controls;
using Arbatel.UI;
using Eto;
using Eto.Forms;
using Eto.OpenTK;
using Eto.OpenTK.Gtk;
using OpenTK;
using System;

namespace Arbatel.Gtk
{
	public static class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

			var platform = new Eto.GtkSharp.Platform();

			platform.Add<GLSurface.IHandler>(() => new GtkGlSurfaceHandler());

			Style.Add<View>(
				"hidecursor",
				view =>
				{
					Gdk.Window window = view.ToNative().GdkWindow;

					var pixmap = new Gdk.Pixmap(null, 1, 1, 1);
					var cursor = new Gdk.Cursor(pixmap, pixmap, Gdk.Color.Zero, Gdk.Color.Zero, 0, 0);

					Gdk.EventMask mask = Gdk.EventMask.PointerMotionMask | Gdk.EventMask.ButtonPressMask;

					// Doesn't successfully limit mouse motion when running in
					// Windows, but that's an edge case anyway, so no big deal.
					Gdk.Pointer.Grab(window, true, mask, window, cursor, 0);
				});

			Style.Add<View>(
				"showcursor",
				view =>
				{
					Gdk.Pointer.Ungrab(0);
				});

			var application = new Application(platform);
			application.UnhandledException += Core.UnhandledExceptionHandler;

			using (opentk)
			{
				application.Run(new MainForm());
			}
		}
	}
}
