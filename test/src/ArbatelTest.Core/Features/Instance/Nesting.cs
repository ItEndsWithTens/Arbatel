using Arbatel.Formats;
using Arbatel.Formats.Quake;
using Arbatel.UI;
using Arbatel.Utilities;
using Eto.Drawing;
using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ArbatelTest.Core.Features.Instance
{
	public class Nesting
	{
		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureDictionary Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpNested
		{
			[OneTimeSetUp]
			public void SetUp()
			{
				DataDirectory = TestContext.Parameters.Get("dataDirectory");
				FgdDirectory = TestContext.Parameters.Get("fgdDirectory");

				string ericwFilename = Path.Combine(FgdDirectory, "quake4ericwTools.fgd");
				var ericw = new QuakeFgd(ericwFilename);

				string instanceFilename = Path.Combine(FgdDirectory, "func_instance.fgd");
				var instance = new QuakeFgd(instanceFilename);

				Fgd = new List<DefinitionDictionary>() { ericw, instance }.Stack();

				string paletteName = "palette-quake.lmp";
				Stream stream = Assembly.GetAssembly(typeof(MainForm)).GetResourceStream(paletteName);
				Palette palette = new Palette().LoadQuakePalette(stream);

				string wadFilename = Path.Combine(DataDirectory, "test.wad");
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
				Filename = Path.Combine(DataDirectory, "instance", "arrow_wedge.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd);
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				MapObject wedge = Map.MapObjects[1];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(256, 0, 64);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightPositionIsCorrect()
			{
				MapObject light = Map.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));

				var expected = new Vector3(256, 0, 160);

				Assert.Multiple(() =>
				{
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
				Filename = Path.Combine(DataDirectory, "instance", "arrow_wedge_holder.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd);
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				MapObject instance = Map.MapObjects[1];

				Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));

				var expected = new Vector3(512, 32, 0);

				Assert.Multiple(() =>
				{
					Assert.That(instance.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(instance.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(instance.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void WedgePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject wedge = collapsed.MapObjects[1];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(512, -224, 64);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightPositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));

				var expected = new Vector3(512, -224, 160);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightOriginKeyIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));

				var expected = new Vector3(512, -224, 160);
				Vector3 actual = Formatting.StringToVector3(light.KeyVals["origin"].Value);

				Assert.Multiple(() =>
				{
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
				Filename = Path.Combine(DataDirectory, "instance_test-arrow_wedge.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd);
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				MapObject instance = Map.MapObjects[1];

				Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));

				var expected = new Vector3(128, 128, 128);

				Assert.Multiple(() =>
				{
					Assert.That(instance.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(instance.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(instance.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void WedgePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject wedge = collapsed.MapObjects[1];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(-384, 352, 192);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightPositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));

				var expected = new Vector3(-384, 352, 288);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightOriginKeyIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));

				var expected = new Vector3(-384, 352, 288);
				Vector3 actual = Formatting.StringToVector3(light.KeyVals["origin"].Value);

				Assert.Multiple(() =>
				{
					Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}
		}
	}
}
