using Eto;
using NUnit.Framework;

namespace ArbatelTest.Core.Features.Instance
{
	[SetUpFixture]
	public class InitializePlatform
	{
		[OneTimeSetUp]
		public void SetUp()
		{
			Platform platform = Platform.Detect;

			Platform.Initialize(platform);
		}
	}
}
