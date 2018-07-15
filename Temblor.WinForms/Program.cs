using System;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Windows;
using Temblor.Controls;

namespace Temblor.WinForms
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var platform = Platform.Detect;

			platform.Add<GLSurface.IHandler>(() => new WinGLSurfaceHandler());

			Style.Add<View>(
				"hidecursor",
				handler =>
				{
					System.Windows.Forms.Cursor.Hide();

					CaptureCursor(handler);
				});

			Style.Add<View>(
				"showcursor",
				handler =>
				{
					System.Windows.Forms.Cursor.Show();

					System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle();
				});

			new Application(platform).Run(new MainForm());
		}

		public static void CaptureCursor(View view)
		{
			var center = new Point(view.PointToScreen(view.Bounds.Center));

			System.Windows.Forms.Cursor.Position = center.ToSD();

			var topLeft = new Point(view.PointToScreen(view.Location));

			var clip = new System.Drawing.Rectangle(topLeft.ToSD(), view.Size.ToSD());

			System.Windows.Forms.Cursor.Clip = clip;
		}
	}
}
