using Eto.Drawing;
using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Formats.Quake;

namespace TemblorTest.Core.Features
{
	public static class Helpers
	{
		public static Vector3 StringToVector3(string value)
		{
			var vector = new Vector3();

			string[] split = value.Split(' ');
			float.TryParse(split[0], out vector.X);
			float.TryParse(split[1], out vector.Y);
			float.TryParse(split[2], out vector.Z);

			return vector;
		}
	}

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

	public class InstanceTest
	{
		public static char Sep { get; private set; }

		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }
		public static string ResourceDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureCollection Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpInstanceTest
		{
			[OneTimeSetUp]
			public void SetUp()
			{
				Assembly assembly = Assembly.GetExecutingAssembly();

				Sep = Path.DirectorySeparatorChar;

				DataDirectory =
					Path.GetDirectoryName(assembly.Location) + Sep
					+ ".." + Sep + ".." + Sep
					+ "data" + Sep;

				FgdDirectory =
					Path.GetDirectoryName(assembly.Location) + Sep
					+ ".." + Sep + ".." + Sep + ".." + Sep
					+ "extras" + Sep;

				ResourceDirectory =
					Path.GetDirectoryName(assembly.Location) + Sep
					+ ".." + Sep + ".." + Sep + ".." + Sep
					+ "res" + Sep;

				var ericwFilename = FgdDirectory + "quake4ericwTools.fgd";
				var ericw = new QuakeFgd(ericwFilename);

				var instanceFilename = FgdDirectory + "func_instance.fgd";
				var instance = new QuakeFgd(instanceFilename);

				Fgd = new List<DefinitionDictionary>() { ericw, instance }.Stack();

				var paletteFilename = ResourceDirectory + "paletteQ.lmp";
				var palette = new Palette().LoadQuakePalette(paletteFilename);

				var wadFilename = DataDirectory + "test.wad";
				Textures = new Wad2(wadFilename, palette);
			}
		}

		[TestFixture]
		public class BottomLevelMapLoadsCorrectly
		{
			public string Filename { get; private set; }
			public Map Map { get; private set; }

			[SetUp]
			public void SetUp()
			{
				Filename = DataDirectory + "instance" + Sep + "arrow_wedge.map";
				Map = new QuakeMap(Filename, Fgd, Textures);
			}

			[TestCase]
			public void WedgeIsPositionedCorrectly()
			{
				var wedge = Map.MapObjects[1];

				var expected = new Vector3(256, 0, 64);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightIsPositionedCorrectly()
			{
				var light = Map.MapObjects[2];

				var expected = new Vector3(256, 0, 160);

				Assert.Multiple(() =>
				{
					Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}
		}

		[TestFixture]
		public class MidLevelMapLoadsCorrectly
		{
			public string Filename { get; private set; }
			public QuakeMap Map { get; private set; }

			[SetUp]
			public void SetUp()
			{
				Filename = DataDirectory + "instance" + Sep + "arrow_wedge_holder.map";
				Map = new QuakeMap(Filename, Fgd, Textures);
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				var instance = Map.MapObjects[1];

				var expected = new Vector3(512, 32, 0);

				Assert.Multiple(() =>
				{
					Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));
					Assert.That(instance.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(instance.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(instance.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void WedgePositionIsCorrectAfterCollapse()
			{
				var collapsed = Map.Collapse();

				var wedge = collapsed.MapObjects[1];

				var expected = new Vector3(512, -224, 64);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightPositionIsCorrectAfterCollapse()
			{
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

				var expected = new Vector3(512, -224, 160);

				Assert.Multiple(() =>
				{
					Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightOriginKeyIsCorrectAfterCollapse()
			{
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

				var expected = new Vector3(512, -224, 160);
				var actual = Helpers.StringToVector3(light.KeyVals["origin"].Value);

				Assert.Multiple(() =>
				{
					Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
					Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}
		}

		[TestFixture]
		public class TopLevelMapLoadsCorrectly
		{
			public string Filename { get; private set; }
			public QuakeMap Map { get; private set; }

			[SetUp]
			public void SetUp()
			{
				Filename = DataDirectory + "instance_test-arrow_wedge.map";
				Map = new QuakeMap(Filename, Fgd, Textures);
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				var instance = Map.MapObjects[1];

				var expected = new Vector3(128, 128, 128);

				Assert.Multiple(() =>
				{
					Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));
					Assert.That(instance.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(instance.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(instance.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void WedgePositionIsCorrectAfterCollapse()
			{
				var collapsed = Map.Collapse();

				var wedge = collapsed.MapObjects[1];

				var expected = new Vector3(-384, 352, 192);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightPositionIsCorrectAfterCollapse()
			{
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

				var expected = new Vector3(-384, 352, 288);

				Assert.Multiple(() =>
				{
					Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightOriginKeyIsCorrectAfterCollapse()
			{
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

				var expected = new Vector3(-384, 352, 288);
				var actual = Helpers.StringToVector3(light.KeyVals["origin"].Value);

				Assert.Multiple(() =>
				{
					Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
					Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}
		}
	}
}
