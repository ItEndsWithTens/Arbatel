using Eto;
using Eto.Forms;
using Eto.Veldrid;
using OpenTK;

namespace Arbatel
{
	public static partial class Core
	{
		// In the long term, the goal is to use Veldrid's implementation of
		// all its supported graphics APIs. For now, though, Arbatel's code
		// doesn't run as well (or as correctly) in Veldrid's OpenGL as it
		// does in straight OpenTK, so skip Veldrid in the case of OpenGL.
		//
		// VeldridSurface does have provisions for OpenGL support, mind you,
		// and an option for users to manually specify what API they want is
		// coming to the settings dialog box sooner or later.
		public static bool UseVeldrid { get; } = VeldridSurface.PreferredBackend != Veldrid.GraphicsBackend.OpenGL;

		public static Toolkit InitOpenTK()
		{
			// Prevent OpenTK from using SDL2 when it's available. The toolkit
			// tries to initialize that first, by default, not caring that it
			// doesn't support GLControl. Stepping through the code showed that
			// OpenTK.Configuration.RunningOnSdl2 was true. A simple search then
			// revealed the answer: https://github.com/opentk/opentk/issues/266
			var options = new ToolkitOptions() { Backend = PlatformBackend.PreferNative };

			// An important point is that when embedding OpenTK in a GUI toolkit
			// like Eto, not only must this method be called, but it must be the
			// very first thing done by your application. Attempting to use it
			// later can cause issues that are hard to trace, e.g. the program
			// crashing when first adding a menu with text to a MenuBar.
			return Toolkit.Init(options);
		}

		public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is GraphicsException ge)
			{
				MessageBox.Show(ge.Message);
			}
		}
	}
}
