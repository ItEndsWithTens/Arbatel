using Arbatel.Formats;
using Arbatel.Formats.Quake;
using Arbatel.Graphics;
using Arbatel.UI;
using Arbatel.Utilities;
using Eto.Drawing;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ArbatelTest.Core.NeedsEto.Features.MiscExternalMap
{
	public class Scale
	{
		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureDictionary Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpScale
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
		public class Collapse
		{
			public string Filename { get; private set; }
			public Map Map { get; private set; }

			[SetUp]
			public void SetUp()
			{
				Filename = Path.Combine(DataDirectory, "external_map_test-scale.map");

				string raw = File.ReadAllText(Filename);

				raw = raw.Replace("REPLACE_TO_MAKE_ABSOLUTE_PATH", Path.GetDirectoryName(Path.GetFullPath(Filename)));

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(raw)))
				{
					Map = new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void HaveExternalMapPointEntitiesButNoMonstersBeforeCollapse()
			{
				Assert.That(Map.MapObjects.Count, Is.EqualTo(5));

				Assert.Multiple(() =>
				{
					Assert.That(Map.MapObjects[0].Definition.ClassName, Is.EqualTo("worldspawn"));
					Assert.That(Map.MapObjects[1].Definition.ClassName, Is.EqualTo("misc_external_map"));
					Assert.That(Map.MapObjects[2].Definition.ClassName, Is.EqualTo("misc_external_map"));
					Assert.That(Map.MapObjects[3].Definition.ClassName, Is.EqualTo("misc_external_map"));
					Assert.That(Map.MapObjects[4].Definition.ClassName, Is.EqualTo("misc_external_map"));
				});
			}

			[TestCase]
			public void HaveOnlyWorldspawnBrushesAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				Assert.That(collapsed.MapObjects.Count, Is.EqualTo(1));

				MapObject worldspawn = Map.MapObjects[0];

				Assert.Multiple(() =>
				{
					Assert.That(worldspawn.Definition.ClassName, Is.EqualTo("worldspawn"));
					Assert.That(worldspawn.Children.Count, Is.EqualTo(0));
					Assert.That(worldspawn.Renderables.Count, Is.EqualTo(4));
				});
			}

			[TestCase]
			public void BrushesAreScaledCorrectlyBeforeCollapse()
			{
				Vertex first = Map.MapObjects[1].Children[0].Renderables[0].Vertices[0];
				Vertex last = Map.MapObjects[1].Children[0].Renderables[0].Vertices[23];

				Polygon p = Map.MapObjects[1].Children[0].Renderables[0].Polygons[0];

				Assert.Multiple(() =>
				{
					Assert.That(first.Position.X, Is.EqualTo(272.0f).Within(Tolerance));
					Assert.That(first.Position.Y, Is.EqualTo(240.0f).Within(Tolerance));
					Assert.That(first.Position.Z, Is.EqualTo(-16.0f).Within(Tolerance));

					Assert.That(last.Position.X, Is.EqualTo(240.0f).Within(Tolerance));
					Assert.That(last.Position.Y, Is.EqualTo(240.0f).Within(Tolerance));
					Assert.That(last.Position.Z, Is.EqualTo(16.0f).Within(Tolerance));

					Assert.That(p.BasisS.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(p.BasisS.Y, Is.EqualTo(2.0f).Within(Tolerance));
					Assert.That(p.BasisS.Z, Is.EqualTo(0.0f).Within(Tolerance));

					Assert.That(p.BasisT.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(p.BasisT.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(p.BasisT.Z, Is.EqualTo(-2.0f).Within(Tolerance));
				});
			}

			[TestCase]
			public void BrushesAreScaledCorrectlyAfterCollapse()
			{
				Map collapsed = Map.Collapse();

				Vertex first = collapsed.MapObjects[0].Renderables[0].Vertices[0];
				Vertex last = collapsed.MapObjects[0].Renderables[0].Vertices[23];

				Polygon p = Map.MapObjects[1].Children[0].Renderables[0].Polygons[0];

				Assert.Multiple(() =>
				{
					Assert.That(first.Position.X, Is.EqualTo(272.0f).Within(Tolerance));
					Assert.That(first.Position.Y, Is.EqualTo(240.0f).Within(Tolerance));
					Assert.That(first.Position.Z, Is.EqualTo(-16.0f).Within(Tolerance));

					Assert.That(last.Position.X, Is.EqualTo(240.0f).Within(Tolerance));
					Assert.That(last.Position.Y, Is.EqualTo(240.0f).Within(Tolerance));
					Assert.That(last.Position.Z, Is.EqualTo(16.0f).Within(Tolerance));

					Assert.That(p.BasisS.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(p.BasisS.Y, Is.EqualTo(2.0f).Within(Tolerance));
					Assert.That(p.BasisS.Z, Is.EqualTo(0.0f).Within(Tolerance));

					Assert.That(p.BasisT.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(p.BasisT.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(p.BasisT.Z, Is.EqualTo(-2.0f).Within(Tolerance));
				});
			}
		}
	}
}
