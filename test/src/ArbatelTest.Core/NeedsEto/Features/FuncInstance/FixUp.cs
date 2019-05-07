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

namespace ArbatelTest.Core.NeedsEto.Features.FuncInstance
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
					Map = (QuakeMap)new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void UserDefinedPrefix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(64, -64, 64), Tolerance));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("APrefixFor-MyFunnyTargetname"));
			}

			[TestCase]
			public void UserDefinedPostfix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(64, 64, 64), Tolerance));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("MyFunnyTargetname-HasAPostfix"));
			}

			[TestCase]
			public void UserDefinedNoFixup()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(-32, 0, 64), Tolerance));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("MyFunnyTargetname"));
			}

			[TestCase]
			public void AutoDefinedPrefix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(32, 0, 64), Tolerance));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("AutoInstance0MyFunnyTargetname"));
			}

			[TestCase]
			public void AutoDefinedPostfix()
			{
				Map collapsed = Map.Collapse();

				MapObject light = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "light" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(96, 0, 64), Tolerance));

				string actualName = light.KeyVals["targetname"].Value;
				Assert.That(actualName, Is.EqualTo("MyFunnyTargetnameAutoInstance1"));
			}

			[TestCase]
			public void MultipleEntitiesAutoDefinedGetSameNumber()
			{
				Map collapsed = Map.Collapse();

				MapObject spikeface = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "monster_demon1" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(512, 128, 0), Tolerance));

				MapObject blobbie = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "monster_tarbaby" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(640, 0, 0), Tolerance));

				MapObject fishlips = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "monster_fish" &&
					MathUtilities.ApproximatelyEquivalent(
						o.Position, new Vector3(512, -128, 0), Tolerance));

				string actualNameSpikeface = spikeface.KeyVals["targetname"].Value;
				string actualNameBlobbie = blobbie.KeyVals["targetname"].Value;
				string actualNameFishlips = fishlips.KeyVals["targetname"].Value;

				Assert.Multiple(() =>
				{
					Assert.That(actualNameSpikeface, Is.EqualTo("AutoInstance3spikeface"));
					Assert.That(actualNameBlobbie, Is.EqualTo("AutoInstance3blobbie"));
					Assert.That(actualNameFishlips, Is.EqualTo("AutoInstance3fishlips"));
				});
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
					Map = (QuakeMap)new QuakeMap(stream, Fgd).Parse();
				}
			}

			[TestCase]
			public void MaterialIsReplaced()
			{
				Map collapsed = Map.Collapse();

				MapObject worldspawn = collapsed.MapObjects.Find(o =>
					o.Definition.ClassName == "worldspawn");

				Renderable r = worldspawn.Renderables[1];

				Assert.That(r.Polygons[0].IntendedTextureName, Is.EqualTo("arrow_red"));
			}
		}
	}
}
