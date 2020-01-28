using Arbatel.Controls;
using Arbatel.UI;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using OpenTK;
using System;

namespace Arbatel.WinForms
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

			Platform platform = Platform.Detect;

			Style.Add<View>(
				"hidecursor",
				view =>
				{
					System.Windows.Forms.Cursor.Hide();

					var center = new Point(view.PointToScreen(view.Bounds.Center));

					System.Windows.Forms.Cursor.Position = center.ToSD();

					var topLeft = new Point(view.PointToScreen(view.Location));

					var clip = new System.Drawing.Rectangle(topLeft.ToSD(), view.Size.ToSD());

					System.Windows.Forms.Cursor.Clip = clip;
				});

			Style.Add<View>(
				"showcursor",
				view =>
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
	}
}
