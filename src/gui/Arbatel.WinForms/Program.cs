using Arbatel.Controls;
using Arbatel.UI;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Gl;
using Eto.Gl.Windows;
using OpenTK;
using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace Arbatel.WinForms
{
	public class PuppetWinGLSurfaceHandler : WinGLSurfaceHandler
	{
		public override void AttachEvent(string id)
		{
			switch (id)
			{
				// Prevent the base surface handler class from attaching its own
				// internal event handler to these events; said handler calls
				// MakeCurrent, uses GL.Viewport, and swaps buffers. That's
				// undesirable here, so just attach the appropriate callback.
				case GLSurface.ShownEvent:
					break;
				case GLSurface.GLDrawEvent:
					Control.Paint += (sender, e) => Callback.OnDraw(Widget, EventArgs.Empty);
					break;
				case GLSurface.SizeChangedEvent:
					Control.SizeChanged += (sender, e) => Callback.OnSizeChanged(Widget, EventArgs.Empty);
					break;
				default:
					base.AttachEvent(id);
					break;
			}
		}
	}

	public class WinFormsVeldridSurfaceHandler : VeldridSurfaceHandler
	{
		protected override void InitializeOpenGL()
		{
			Platform.Instance.Add<GLSurface.IHandler>(() => new PuppetWinGLSurfaceHandler());

			base.InitializeOpenGL();
		}

		protected override void InitializeOtherApi()
		{
			// OpenGL initialization is technically platform-dependent, but it
			// happens by way of GLSurface, which for users of the class is
			// cross platform. See VeldridSurface for initialization details.
			if (Widget.Backend == GraphicsBackend.Vulkan)
			{
				Widget.GraphicsDevice = GraphicsDevice.CreateVulkan(new GraphicsDeviceOptions());
			}
			else if (Widget.Backend == GraphicsBackend.Direct3D11)
			{
				Widget.GraphicsDevice = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());
			}
			else
			{
				string message;
				if (!Enum.IsDefined(typeof(GraphicsBackend), Widget.Backend))
				{
					message = "Unrecognized backend!";
				}
				else
				{
					message = "Specified backend not supported on this platform!";
				}

				throw new ArgumentException(message);
			}

			var source = SwapchainSource.CreateWin32(
				Control.NativeHandle, Marshal.GetHINSTANCE(typeof(VeldridSurface).Module));
			Widget.Swapchain = Widget.GraphicsDevice.ResourceFactory.CreateSwapchain(
				new SwapchainDescription(
					source,
					(uint)Widget.Width,
					(uint)Widget.Height,
					Veldrid.PixelFormat.R32_Float,
					false));
		}
	}

	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Toolkit opentk = Core.InitOpenTK();

			Platform platform = Platform.Detect;

			// If Veldrid will be used as the backend for this application, an
			// instance of PuppetWinGLSurfaceHandler will need to be used as the
			// handler for GLSurface. Unfortunately there's no way to determine
			// whether Veldrid is suitable or not just yet, so add the normal
			// handler first and update it later. Eto.Platform.Add overwrites
			// existing control handlers, so there's no worry about duplicates.
			platform.Add<GLSurface.IHandler>(() => new WinGLSurfaceHandler());
			platform.Add<VeldridSurface.IHandler>(() => new WinFormsVeldridSurfaceHandler());

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
