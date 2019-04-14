using Arbatel.Formats;
using Arbatel.Formats.Quake;
using Arbatel.Graphics;
using Arbatel.UI;
using Arbatel.Utilities;
using Eto.Drawing;
using NUnit.Framework;
using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ArbatelTest.Core.NeedsEto.Features.Instance
{
	public class Pitch
	{
		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureDictionary Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpPitch
		{
			[OneTimeSetUp]
			public void SetUp()
			{
				DataDirectory = TestContext.Parameters["dataDirectory"];
				FgdDirectory = TestContext.Parameters["fgdDirectory"];

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
				Filename = Path.Combine(DataDirectory, "instance", "rotation_instance.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				MapObject wedge = Map.MapObjects[3];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(128, 0, 0);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithManglePositionIsCorrect()
			{
				MapObject light = Map.MapObjects[1];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("has_mangle"));

				var expected = new Vector3(128, 128, 0);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutManglePositionIsCorrect()
			{
				MapObject light = Map.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				var expected = new Vector3(128, -128, 0);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
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
				Filename = Path.Combine(DataDirectory, "instance_test-pitch.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = (QuakeMap)new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				MapObject instance = Map.MapObjects[1];

				Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));

				var expected = new Vector3(0, 0, 0);

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

				MapObject wedge = collapsed.MapObjects[3];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(94.8512497f, 0.0f, 64.0f);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithManglePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[1];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("has_mangle"));

				var expected = new Vector3(110.8512517f, 128.0f, 64.0f);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutManglePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				var expected = new Vector3(110.8512517f, -128.0f, 64.0f);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutMangleRenderableVerticesAreCorrectAfterCollapse()
			{
				var collapsed = (QuakeMap)Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				Renderable box = light.Renderables[0];

				// Should be at 120, -136, -8 before rotation.
				Vertex vertex0 = box.Vertices[0].ModelToWorld(box.ModelMatrix);
				var expected0 = new Vector3(102.8512517f, -136.0f, 56.0f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex0.Position.X, Is.EqualTo(expected0.X).Within(Tolerance));
					Assert.That(vertex0.Position.Y, Is.EqualTo(expected0.Y).Within(Tolerance));
					Assert.That(vertex0.Position.Z, Is.EqualTo(expected0.Z).Within(Tolerance));
				});

				// Should be at 136, -120, 8 before rotation.
				Vertex vertex18 = box.Vertices[18].ModelToWorld(box.ModelMatrix);
				var expected18 = new Vector3(118.8512517f, -120.0f, 72.0f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex18.Position.X, Is.EqualTo(expected18.X).Within(Tolerance));
					Assert.That(vertex18.Position.Y, Is.EqualTo(expected18.Y).Within(Tolerance));
					Assert.That(vertex18.Position.Z, Is.EqualTo(expected18.Z).Within(Tolerance));
				});
			}
		}
	}

	public class Yaw
	{
		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureDictionary Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpYaw
		{
			[OneTimeSetUp]
			public void SetUp()
			{
				DataDirectory = TestContext.Parameters["dataDirectory"];
				FgdDirectory = TestContext.Parameters["fgdDirectory"];

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
				Filename = Path.Combine(DataDirectory, "instance", "rotation_instance.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				MapObject wedge = Map.MapObjects[3];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(128, 0, 0);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithManglePositionIsCorrect()
			{
				MapObject light = Map.MapObjects[1];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("has_mangle"));

				var expected = new Vector3(128, 128, 0);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutManglePositionIsCorrect()
			{
				MapObject light = Map.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				var expected = new Vector3(128, -128, 0);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
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
				Filename = Path.Combine(DataDirectory, "instance_test-yaw.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = (QuakeMap)new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				MapObject instance = Map.MapObjects[1];

				Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));

				var expected = new Vector3(0, 0, 0);

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

				MapObject wedge = collapsed.MapObjects[3];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(119.497398f, 24.8466263f, 0.0f);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithManglePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[1];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("has_mangle"));

				var expected = new Vector3(90.5096680f, 156.7673435f, 0.0f);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutManglePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				var expected = new Vector3(156.7673435f, -90.5096680f, 0.0f);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutMangleRenderableVerticesAreCorrectAfterCollapse()
			{
				var collapsed = (QuakeMap)Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				Renderable box = light.Renderables[0];

				// Should be at 120, -136, -8 before rotation.
				Vertex vertex0 = box.Vertices[0].ModelToWorld(box.ModelMatrix);
				var expected0 = new Vector3(148.7673435f, -98.509668f, -8.0f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex0.Position.X, Is.EqualTo(expected0.X).Within(Tolerance));
					Assert.That(vertex0.Position.Y, Is.EqualTo(expected0.Y).Within(Tolerance));
					Assert.That(vertex0.Position.Z, Is.EqualTo(expected0.Z).Within(Tolerance));
				});

				// Should be at 136, -120, 8 before rotation.
				Vertex vertex18 = box.Vertices[18].ModelToWorld(box.ModelMatrix);
				var expected18 = new Vector3(164.7673435f, -82.509668f, 8.0f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex18.Position.X, Is.EqualTo(expected18.X).Within(Tolerance));
					Assert.That(vertex18.Position.Y, Is.EqualTo(expected18.Y).Within(Tolerance));
					Assert.That(vertex18.Position.Z, Is.EqualTo(expected18.Z).Within(Tolerance));
				});
			}
		}
	}

	public class Roll
	{
		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureDictionary Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpRoll
		{
			[OneTimeSetUp]
			public void SetUp()
			{
				DataDirectory = TestContext.Parameters["dataDirectory"];
				FgdDirectory = TestContext.Parameters["fgdDirectory"];

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
				Filename = Path.Combine(DataDirectory, "instance", "rotation_instance.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				MapObject wedge = Map.MapObjects[3];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(128, 0, 0);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithManglePositionIsCorrect()
			{
				MapObject light = Map.MapObjects[1];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("has_mangle"));

				var expected = new Vector3(128, 128, 0);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutManglePositionIsCorrect()
			{
				MapObject light = Map.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				var expected = new Vector3(128, -128, 0);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
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
				Filename = Path.Combine(DataDirectory, "instance_test-roll.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = (QuakeMap)new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				MapObject instance = Map.MapObjects[1];

				Assert.That(instance.Definition.ClassName, Is.EqualTo("func_instance"));

				var expected = new Vector3(0, 0, 0);

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

				MapObject wedge = collapsed.MapObjects[3];

				Assert.That(wedge.Definition.ClassName, Is.EqualTo("func_wall"));

				var expected = new Vector3(128.0f, 0.0f, 0.0f);

				Assert.Multiple(() =>
				{
					Assert.That(wedge.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(wedge.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(wedge.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithManglePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[1];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("has_mangle"));

				var expected = new Vector3(128.0f, 120.2806555f, -43.7785784f);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutManglePositionIsCorrectAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				var expected = new Vector3(128.0f, -120.2806555f, 43.7785784f);

				Assert.Multiple(() =>
				{
					Assert.That(light.Position.X, Is.EqualTo(expected.X).Within(Tolerance));
					Assert.That(light.Position.Y, Is.EqualTo(expected.Y).Within(Tolerance));
					Assert.That(light.Position.Z, Is.EqualTo(expected.Z).Within(Tolerance));
				});
			}

			[TestCase]
			public void LightWithoutMangleRenderableVerticesAreCorrectAfterCollapse()
			{
				var collapsed = (QuakeMap)Map.Collapse();

				MapObject light = collapsed.MapObjects[2];

				Assert.That(light.Definition.ClassName, Is.EqualTo("light"));
				Assert.That(light.KeyVals["targetname"].Value, Is.EqualTo("no_mangle"));

				Renderable box = light.Renderables[0];

				// Should be at 120, -136, -8 before rotation.
				Vertex vertex0 = box.Vertices[0].ModelToWorld(box.ModelMatrix);
				var expected0 = new Vector3(120.0f, -128.2806555f, 35.7785784f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex0.Position.X, Is.EqualTo(expected0.X).Within(Tolerance));
					Assert.That(vertex0.Position.Y, Is.EqualTo(expected0.Y).Within(Tolerance));
					Assert.That(vertex0.Position.Z, Is.EqualTo(expected0.Z).Within(Tolerance));
				});

				// Should be at 136, -120, 8 before rotation.
				Vertex vertex18 = box.Vertices[18].ModelToWorld(box.ModelMatrix);
				var expected18 = new Vector3(136.0f, -112.2806555f, 51.7785784f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex18.Position.X, Is.EqualTo(expected18.X).Within(Tolerance));
					Assert.That(vertex18.Position.Y, Is.EqualTo(expected18.Y).Within(Tolerance));
					Assert.That(vertex18.Position.Z, Is.EqualTo(expected18.Z).Within(Tolerance));
				});
			}
		}
	}
}
