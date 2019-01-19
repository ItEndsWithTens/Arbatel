using Arbatel;
using Arbatel.UI;
using Eto;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Gtk;
using Eto.Gl.Mac;
using Eto.Gl.Windows;
using OpenTK;
using System;

namespace ArbatelTest.Rendering
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

			Platform platform = Platform.Detect;

			if (EtoEnvironment.Platform.IsMac)
			{
				platform.Add<GLSurface.IHandler>(() => new MacGLSurfaceHandler());
			}
			else if (EtoEnvironment.Platform.IsLinux)
			{
				platform.Add<GLSurface.IHandler>(() => new GtkGlSurfaceHandler());
			}
			else
			{
				platform.Add<GLSurface.IHandler>(() => new WinGLSurfaceHandler());
			}

			var form = new MainForm();

			var testMenu = new ButtonMenuItem
			{
				Text = "Test",
				Items =
				{
					new ButtonMenuItem
					{
						Text = "Get average refresh time",
						Command = Commands.CmdAverageRefreshTime,
						CommandParameter = form
					}
				}
			};

			form.Menu.Items.Add(testMenu);

			var application = new Application(platform);
			application.UnhandledException += Core.UnhandledExceptionHandler;

			using (opentk)
			{
				application.Run(form);
			}
		}
	}
}
