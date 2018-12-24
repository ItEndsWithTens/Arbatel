using Arbatel.Controls;
using Arbatel.UI;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Windows;
using OpenTK;
using System;

namespace Arbatel.WinForms
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

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

			var application = new Application(platform);
			application.UnhandledException += Core.UnhandledExceptionHandler;

			var mainForm = new MainForm();

			// For whatever reason, WinForms converts the Control | Oemcomma
			// shortcut key into the string "Ctrl+Oemcomma" instead of "Ctrl+,",
			// so it needs to be poked with a stick to look presentable.
			var editMenu = (ButtonMenuItem)mainForm.Menu.Items[1];
			MenuItem preferencesItem = editMenu.Items[0];
			string intendedShortcutText = preferencesItem.Shortcut.ToShortcutString();
			var native = (System.Windows.Forms.ToolStripMenuItem)preferencesItem.ControlObject;
			native.ShortcutKeyDisplayString = intendedShortcutText;

			using (opentk)
			{
				application.Run(mainForm);
			}
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
