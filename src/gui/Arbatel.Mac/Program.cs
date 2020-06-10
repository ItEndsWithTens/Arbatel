using Arbatel.Controls;
using Arbatel.UI;
using Eto;
using Eto.Forms;
using OpenTK;
using System;
using System.Runtime.InteropServices;

namespace Arbatel.Mac
{
	// This, like the other Quartz display stuff further below, was lifted from
	// OpenTK. See OpenTK/Platform/MacOS/Quartz/DisplayServices.cs for the
	// origins of these definitions.
	using CGDirectDisplayID = IntPtr;

	internal enum CGError
	{
		Success = 0,
		Failure = 1000,
		IllegalArgument = 1001,
		InvalidConnection = 1002,
		InvalidContext = 1003,
		CannotComplete = 1004,
		NotImplemented = 1006,
		RangeCheck = 1007,
		TypeCheck = 1008,
		InvalidOperation = 1010,
		NoneAvailable = 1011
	}

	// As per the aforementioned OpenTK source file, the methods used here are
	// only available in macOS 10.3 or later.
	internal static class CG
	{
		public const string Library = "/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices";

		// Docs taken from https://developer.apple.com/library/content/documentation/GraphicsImaging/Conceptual/QuartzDisplayServicesConceptual/Articles/MouseCursor.html

		/// <summary>
		/// Connects or disconnects the mouse and cursor while an application is in the foreground.
		/// </summary>
		/// <returns>A result code.</returns>
		/// <param name="connected">Pass true if the mouse and cursor should be connected; otherwise, pass false.</param>
		[DllImport(Library, EntryPoint = "CGAssociateMouseAndMouseCursorPosition")]
		public static extern CGError AssociateMouseAndMouseCursorPosition(bool connected);

		/// <summary>
		/// Hides the mouse cursor, and increments the hide cursor count.
		/// </summary>
		/// <returns>A result code.</returns>
		/// <param name="display">This parameter is not used.</param>
		[DllImport(Library, EntryPoint = "CGDisplayHideCursor")]
		public static extern CGError DisplayHideCursor(CGDirectDisplayID display);

		/// <summary>
		/// Decrements the hide cursor count, and shows the mouse cursor if the count is 0.
		/// </summary>
		/// <returns>A result code.</returns>
		/// <param name="display">This parameter is not used.</param>
		[DllImport(Library, EntryPoint = "CGDisplayShowCursor")]
		public static extern CGError DisplayShowCursor(CGDirectDisplayID display);
	}

	public static class MainClass
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
					CG.DisplayHideCursor((CGDirectDisplayID)0);
					CG.AssociateMouseAndMouseCursorPosition(false);
				});

			Style.Add<View>(
				"showcursor",
				view =>
				{
					CG.AssociateMouseAndMouseCursorPosition(true);
					CG.DisplayShowCursor((CGDirectDisplayID)0);
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
