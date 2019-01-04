using Eto;
using NUnit.Framework;
using System;

namespace ArbatelTest.Core
{
	// Tests are now implemented as an executable using the NUnitLite runner,
	// instead of as a class library using the NUnit3 console runner, to allow
	// Eto.Forms to properly detect and initialize the platform.
	class MainClass
	{
		[STAThread]
		public static int Main(string[] args)
		{
			Platform platform = Platform.Detect;

			Platform.Initialize(platform);

			Console.WriteLine($"\nUsing Eto platform {platform.ID} for test execution...\n");

			return new NUnitLite.AutoRun().Execute(args);
		}
	}
}
