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
using Temblor.Graphics;
using Temblor.Utilities;

namespace TemblorTest.Core.Features.Instance
{
	public class Pitch
	{
		public static char Sep { get; private set; }

		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }
		public static string ResourceDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureCollection Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpPitch
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
				Filename = DataDirectory + "instance" + Sep + "rotation_instance.map";

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd, Textures);
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				var wedge = Map.MapObjects[3];

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
				var light = Map.MapObjects[1];

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
				var light = Map.MapObjects[2];

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
				Filename = DataDirectory + "instance_test-pitch.map";

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd, Textures);
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				var instance = Map.MapObjects[1];

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
				var collapsed = Map.Collapse();

				var wedge = collapsed.MapObjects[3];

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
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[1];

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
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

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
				QuakeMap collapsed = Map.Collapse();

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
				Vertex vertex6 = box.Vertices[6].ModelToWorld(box.ModelMatrix);
				var expected6 = new Vector3(118.8512517f, -120.0f, 72.0f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex6.Position.X, Is.EqualTo(expected6.X).Within(Tolerance));
					Assert.That(vertex6.Position.Y, Is.EqualTo(expected6.Y).Within(Tolerance));
					Assert.That(vertex6.Position.Z, Is.EqualTo(expected6.Z).Within(Tolerance));
				});
			}
		}
	}

	public class Yaw
	{
		public static char Sep { get; private set; }

		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }
		public static string ResourceDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureCollection Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpYaw
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
				Filename = DataDirectory + "instance" + Sep + "rotation_instance.map";

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd, Textures);
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				var wedge = Map.MapObjects[3];

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
				var light = Map.MapObjects[1];

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
				var light = Map.MapObjects[2];

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
				Filename = DataDirectory + "instance_test-yaw.map";

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd, Textures);
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				var instance = Map.MapObjects[1];

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
				var collapsed = Map.Collapse();

				var wedge = collapsed.MapObjects[3];

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
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[1];

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
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

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
				QuakeMap collapsed = Map.Collapse();

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
				Vertex vertex6 = box.Vertices[6].ModelToWorld(box.ModelMatrix);
				var expected6 = new Vector3(164.7673435f, -82.509668f, 8.0f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex6.Position.X, Is.EqualTo(expected6.X).Within(Tolerance));
					Assert.That(vertex6.Position.Y, Is.EqualTo(expected6.Y).Within(Tolerance));
					Assert.That(vertex6.Position.Z, Is.EqualTo(expected6.Z).Within(Tolerance));
				});
			}
		}
	}

	public class Roll
	{
		public static char Sep { get; private set; }

		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }
		public static string ResourceDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureCollection Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpRoll
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
				Filename = DataDirectory + "instance" + Sep + "rotation_instance.map";

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd, Textures);
				}
			}

			[TestCase]
			public void WedgePositionIsCorrect()
			{
				var wedge = Map.MapObjects[3];

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
				var light = Map.MapObjects[1];

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
				var light = Map.MapObjects[2];

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
				Filename = DataDirectory + "instance_test-roll.map";

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd, Textures);
				}
			}

			[TestCase]
			public void InstancePositionIsCorrectBeforeCollapse()
			{
				var instance = Map.MapObjects[1];

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
				var collapsed = Map.Collapse();

				var wedge = collapsed.MapObjects[3];

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
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[1];

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
				var collapsed = Map.Collapse();

				var light = collapsed.MapObjects[2];

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
				QuakeMap collapsed = Map.Collapse();

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
				Vertex vertex6 = box.Vertices[6].ModelToWorld(box.ModelMatrix);
				var expected6 = new Vector3(136.0f, -112.2806555f, 51.7785784f);
				Assert.Multiple(() =>
				{
					Assert.That(vertex6.Position.X, Is.EqualTo(expected6.X).Within(Tolerance));
					Assert.That(vertex6.Position.Y, Is.EqualTo(expected6.Y).Within(Tolerance));
					Assert.That(vertex6.Position.Z, Is.EqualTo(expected6.Z).Within(Tolerance));
				});
			}
		}
	}
}
