using Arbatel.Utilities;
using Eto;
using NUnit.Framework;
using System;

namespace ArbatelTest.Core.NeedsEto
{
	[SetUpFixture]
	public class SetUpEto
	{
		[OneTimeSetUp]
		public void SetUp()
		{
			Platform platform;
			if (EtoEnvironment.Platform.IsMac)
			{
				Type type = AssemblyUtilities.GetTypeFromName("Eto.Mac.Platform");
				platform = (Platform)Activator.CreateInstance(type);
			}
			else
			{
				platform = Platform.Detect;
			}

			Console.WriteLine($"\nUsing Eto platform {platform.ID} for test execution...\n");

			Platform.Initialize(platform);
		}
	}
}
