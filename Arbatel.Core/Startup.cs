using OpenTK;

namespace Arbatel
{
	public static class Startup
	{
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
	}
}
