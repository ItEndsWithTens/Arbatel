using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbatelTest.Core.Features.Instance
{
	[SetUpFixture]
	public class InitializePlatform
	{
		[OneTimeSetUp]
		public void SetUp()
		{
			var platform = Eto.Platform.Detect;

			Eto.Platform.Initialize(platform);
		}
	}
}
