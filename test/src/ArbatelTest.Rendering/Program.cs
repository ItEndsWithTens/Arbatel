using Arbatel;
using Arbatel.UI;
using Arbatel.Utilities;
using Eto;
using Eto.Forms;
using Eto.OpenTK;
using OpenTK;
using System;
using System.Reflection;

namespace ArbatelTest.Rendering
{
	public class Program
	{
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
				Assembly.Load("Eto.OpenTK.Mac64");
				Type type = AssemblyUtilities.GetTypeFromName("Eto.OpenTK.Mac.MacGLSurfaceHandler");
				platform.Add(() => (GLSurface.IHandler)Activator.CreateInstance(type));
			}
			else if (platform.IsGtk)
			{
				Assembly.Load("Eto.OpenTK.Gtk2");
				Type type = AssemblyUtilities.GetTypeFromName("Eto.OpenTK.Gtk.GtkGlSurfaceHandler");
				platform.Add(() => (GLSurface.IHandler)Activator.CreateInstance(type));
			}
			else
			{
				Assembly.Load("Eto.OpenTK.WinForms");
				Type type = AssemblyUtilities.GetTypeFromName("Eto.OpenTK.WinForms.WinGLSurfaceHandler");
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
					},
					new ButtonMenuItem
					{
						Text = "Count visible triangles",
						Command = Commands.CmdVisibleTriangles,
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
