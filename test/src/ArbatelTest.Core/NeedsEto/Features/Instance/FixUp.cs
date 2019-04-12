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
	public class Fixup
	{
		public static string DataDirectory { get; private set; }
		public static string FgdDirectory { get; private set; }

		public static DefinitionDictionary Fgd { get; private set; }

		public static TextureDictionary Textures { get; private set; }

		public static readonly float Tolerance = 0.001f;

		[SetUpFixture]
		public class SetUpFixup
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
		public class TargetnameFixup
		{
			public string Filename { get; private set; }
			public QuakeMap Map { get; private set; }

			[SetUp]
			public void SetUp()
			{
				Filename = Path.Combine(DataDirectory, "instance_test-fixup.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd);
				}
			}

			[TestCase]
			public void UserDefinedPrefix()
			{
				Map collapsed = Map.Collapse();

				// The positions of these lights are precisely defined in their
				// map file, and the coordinates are simply loaded, not produced
				// as the result of any calculation. A simple Vector3 equality
				// check is therefore reliable, as well as easy to read.
				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					o.Position == new Vector3(64, -64, 64));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("APrefixFor-MyFunnyTargetname"));
			}

			[TestCase]
			public void UserDefinedPostfix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					o.Position == new Vector3(64, 64, 64));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("MyFunnyTargetname-HasAPostfix"));
			}

			[TestCase]
			public void UserDefinedNoFixup()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					o.Position == new Vector3(-32, 0, 64));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("MyFunnyTargetname"));
			}

			[TestCase]
			public void AutoDefinedPrefix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					o.Position == new Vector3(32, 0, 64));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("AutoInstance0MyFunnyTargetname"));
			}

			[TestCase]
			public void AutoDefinedPostfix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					o.Position == new Vector3(96, 0, 64));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("MyFunnyTargetnameAutoInstance1"));
			}
		}

		[TestFixture]
		public class MaterialReplacement
		{
			public string Filename { get; private set; }
			public QuakeMap Map { get; private set; }

			[SetUp]
			public void SetUp()
			{
				Filename = Path.Combine(DataDirectory, "instance_test-fixup.map");

				using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
				{
					Map = new QuakeMap(stream, Fgd);
				}
			}

			[TestCase]
			public void MaterialIsReplaced()
			{
				Map collapsed = Map.Collapse();

				MapObject worldspawn = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "worldspawn");

				Renderable r = worldspawn.Renderables[1];

				Assert.That(r.Polygons[0].Texture.Name, Is.EqualTo("arrow_red"));
			}
		}
	}
}
