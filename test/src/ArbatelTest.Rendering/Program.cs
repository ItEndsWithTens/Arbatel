using Arbatel;
using Arbatel.UI;
using Eto;
using Eto.Forms;
using Eto.Gl;
using OpenTK;
using System;
using System.Reflection;

namespace ArbatelTest.Rendering
{
	public class Program
	{
		public static Type GetTypeFromName(string name)
		{
			Type type = null;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = assembly.GetType(name);

				if (type != null)
				{
					break;
				}
			}

			return type;
		}

		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

			Platform platform = Platform.Detect;

			// This project has references to multiple Eto and Eto.Gl platforms,
			// to eliminate the need for separate test GUIs.
			//
			// To avoid the subsequent hassle of unnecessary and potentially
			// irritating to install dependencies, e.g. GtkSharp2 on Windows,
			// those references are conditional, and vary based on what platform
			// is building the code.
			//
			// That in turn means the various handler types, WinGLSurfaceHandler
			// et. al., aren't all available at compile time, leading to this
			// assembly loading acrobatics routine.
			if (platform.IsMac)
			{
				Assembly.Load("Eto.Gl.Mac");
				Type type = GetTypeFromName("Eto.Gl.Mac.MacGLSurfaceHandler");
				platform.Add(() => (GLSurface.IHandler)Activator.CreateInstance(type));
			}
			else if (platform.IsGtk)
			{
				Assembly.Load("Eto.Gl.Gtk2");
				Type type = GetTypeFromName("Eto.Gl.Gtk.GtkGlSurfaceHandler");
				platform.Add(() => (GLSurface.IHandler)Activator.CreateInstance(type));
			}
			else
			{
				Assembly.Load("Eto.Gl.Windows");
				Type type = GetTypeFromName("Eto.Gl.Windows.WinGLSurfaceHandler");
				platform.Add(() => (GLSurface.IHandler)Activator.CreateInstance(type));
			}

			var application = new Application(platform);
			application.UnhandledException += Core.UnhandledExceptionHandler;

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

			using (opentk)
			{
				application.Run(form);
			}
		}
	}
}
