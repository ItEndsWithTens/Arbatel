using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace TemblorTest.Core.FormatsTest
{
	public class BlockTest
	{
		[TestFixture]
		public class QuakeBlockTest
		{
			[TestCase]
			public void BasicParsing()
			{
				var raw = new List<string>()
				{
					"{",
					"( -256 512 512 ) ( -256 512 0 ) ( -256 0 512 ) TRIGGER [ 0 1 0 0 ] [ 0 0 - 1 0 ] 0 1 1",
					"( -768 0 512 ) ( -768 0 0 ) ( -768 512 512 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -256 0 512 ) ( -256 0 0 ) ( -768 0 512 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -768 512 512 ) ( -768 512 0 ) ( -256 512 512 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -768 512 0 ) ( -768 0 0 ) ( -256 512 0 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
					"( -256 0 512 ) ( -768 0 512 ) ( -256 512 512 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
					"}"
				};

				var block = new QuakeBlock(ref raw, 0);
			}

			public class OpenBraceInTextureName
			{
				[TestCase]
				public void ParseSingleBlock()
				{
					// Test block lifted from ad_sepulcher.
					List<string> raw = new List<string>()
					{
						"// entity 1458",
						"{",
						"\"_minlight\" \"25\"",
						"\"classname\" \"func_detail_illusionary\"",
						"\"_phong\" \"1\"",
						"\"spawnflags\" \"32\"",
						"\"_shadow\" \"1\"",
						"// brush 0",
						"{",
						"( -1232 -1076 32 ) ( -1232 -1074 32 ) ( -1232 -1074 -104 ) skip 0 0 0 1.000000 1.000000",
						"( -1280 -1084 -104 ) ( -1280 -1082 -104 ) ( -1280 -1082 32 ) skip 0 0 0 1.000000 1.000000",
						"( -1280 -1082 0 ) ( -1280 -1082 -136 ) ( -1232 -1074 -136 ) {vinehang2b -104 64 0 1.000000 1.000000",
						"( -1280 -1084 -136 ) ( -1280 -1084 0 ) ( -1232 -1076 0 ) {vinehang2b -104 64 0 1.000000 1.000000",
						"( -1280 -1084 0 ) ( -1280 -1082 0 ) ( -1232 -1074 0 ) skip 0 0 0 1.000000 1.000000",
						"( -1280 -1082 -136 ) ( -1280 -1084 -136 ) ( -1232 -1076 -136 ) skip 0 0 0 1.000000 1.000000",
						"}",
						"// brush 1",
						"{",
						"( -1184 -1088 -136 ) ( -1184 -1086 -136 ) ( -1232 -1074 -136 ) skip 0 0 0 1.000000 1.000000",
						"( -1184 -1086 0 ) ( -1184 -1088 0 ) ( -1232 -1076 0 ) skip 0 0 0 1.000000 1.000000",
						"( -1184 -1088 0 ) ( -1184 -1088 -136 ) ( -1232 -1076 -136 ) {vinehang2b -104 64 0 1.000000 1.000000",
						"( -1184 -1086 -136 ) ( -1184 -1086 0 ) ( -1232 -1074 0 ) {vinehang2b -104 64 0 1.000000 1.000000",
						"( -1232 -1076 -104 ) ( -1232 -1074 -104 ) ( -1232 -1074 32 ) skip 0 0 0 1.000000 1.000000",
						"( -1184 -1088 32 ) ( -1184 -1086 32 ) ( -1184 -1086 -104 ) skip 0 0 0 1.000000 1.000000",
						"}",
						"}"
					};

					var block = new QuakeBlock(ref raw, 1);

					Assert.That(block.KeyVals.Count, Is.EqualTo(5));

					var thing = new QuakeMapObject(block);

					var brush = thing.Block.Children[0] as QuakeBlock;
					var side = new QuakeSide(brush.Sides[2]);
					Assert.That(side.TextureName, Is.EqualTo("{vinehang2b"));

					side = new QuakeSide(brush.Sides[3]);
					Assert.That(side.TextureName, Is.EqualTo("{vinehang2b"));
				}
			}
		}
	}

	public class MapTest
	{
		[TestFixture]
		public class QuakeMapTest
		{
			[TestCase]
			public void BasicParsing()
			{
				var raw = new List<string>()
				{
					"{",
					"\"classname\" \"worldspawn\"",
					"\"_gamma\" \"1\"",
					"\"_dirtgain\" \"1\"",
					"\"_dirtscale\" \"1\"",
					"\"_dirtdepth\" \"128\"",
					"\"_dirtmode\" \"0\"",
					"\"_minlight_dirt\" \"-1\"",
					"\"_sunlight2_dirt\" \"-1\"",
					"\"_sunlight_dirt\" \"-1\"",
					"\"_dirt\" \"-1\"",
					"\"_anglescale\" \"0.5\"",
					"\"_range\" \"0.5\"",
					"\"_dist\" \"1\"",
					"\"_sunlight3_color\" \"255 255 255\"",
					"\"_sunlight3\" \"0\"",
					"\"_sunlight2_color\" \"255 255 255\"",
					"\"_sunlight2\" \"0\"",
					"\"_sunlight_color\" \"255 255 255\"",
					"\"_sunlight_penumbra\" \"0\"",
					"\"_sun_mangle\" \"0 -90 0\"",
					"\"_sunlight\" \"0\"",
					"\"light\" \"0\"",
					"\"sounds\" \"1\"",
					"\"worldtype\" \"0\"",
					"\"mapversion\" \"220\"",
					"\"wad\" \"D:\\Projects\\Games\\Maps\\Quake\\common\\wads\\quake.wad\"",
					"\"_generator\" \"J.A.C.K. 1.1.1064 (vpQuake)\"",
					"{",
					"( -256 512 512 ) (-256 512 0 ) (-256 0 512 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"(-768 0 512)(-768 0 0)(-768 512 512) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"(-256 0 512)(-256 0 0)(-768 0 512) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"(-768 512 512)(-768 512 0)(-256 512 512) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"(-768 512 0)(-768 0 0)(-256 512 0) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
					"(-256 0 512)(-768 0 512)(-256 512 512) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
					"}",
					"{",
					"( 256 256 0 ) ( 256 -256 0 ) ( 768 256 0 ) TRIGGER [ 1 0 0 -0 ] [ 0 -1 0 -0 ] 0 1 1",
					"( 512 0 512 ) ( 768 256 0 ) ( 768 -256 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( 512 0 512 ) ( 256 -256 0 ) ( 256 256 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( 512 0 512 ) ( 768 -256 0 ) ( 256 -256 0 ) TRIGGER [ 1 0 0 -0 ] [ 0 0 -1 0 ] 0 1 1",
					"( 512 0 512 ) ( 256 256 0 ) ( 768 256 0 ) TRIGGER [ 1 0 0 -0 ] [ 0 0 -1 0 ] 0 1 1",
					"}",
					"{",
					"( 1536 0 0 ) ( 1024 0 0 ) ( 1536 -512 0 ) TRIGGER [ 1 0 0 -0 ] [ 0 -1 0 -0 ] 0 1 1",
					"( 1408 -384 256 ) ( 1152 -384 256 ) ( 1408 -128 256 ) TRIGGER [ 1 0 0 -0 ] [ 0 -1 0 -0 ] 0 1 1",
					"( 1024 0 0 ) ( 1536 0 0 ) ( 1152 -128 256 ) TRIGGER [ 1 0 0 -0 ] [ 0 0 -1 0 ] 0 1 1",
					"( 1536 -512 0 ) ( 1024 -512 0 ) ( 1408 -384 256 ) TRIGGER [ 1 0 0 -0 ] [ 0 0 -1 0 ] 0 1 1",
					"( 1024 -512 0 ) ( 1024 0 0 ) ( 1152 -384 256 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( 1536 0 0 ) ( 1536 -512 0 ) ( 1408 -128 256 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"}",
					"{",
					"( -1099 -693 0 ) ( -1043 -610 0 ) ( -1182 -749 0 ) TRIGGER [ 1 0 0 0 ] [ 0 - 1 0 0 ] 0 1 1",
					"( -1043 -414 512 ) ( -1024 -512 512 ) ( -1099 -331 512 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
					"( -1043 -610 0 ) ( -1043 -610 512 ) ( -1024 -512 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1099 -693 0 ) ( -1099 -693 512 ) ( -1043 -610 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1182 -749 0 ) ( -1182 -749 512 ) ( -1099 -693 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1280 -768 0 ) ( -1280 -768 512 ) ( -1182 -749 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1378 -749 0 ) ( -1378 -749 512 ) ( -1280 -768 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1461 -693 0 ) ( -1461 -693 512 ) ( -1378 -749 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1517 -610 0 ) ( -1517 -610 512 ) ( -1461 -693 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1536 -512 0 ) ( -1536 -512 512 ) ( -1517 -610 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1517 -414 0 ) ( -1517 -414 512 ) ( -1536 -512 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1461 -331 0 ) ( -1461 -331 512 ) ( -1517 -414 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1378 -275 0 ) ( -1378 -275 512 ) ( -1461 -331 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1280 -256 0 ) ( -1280 -256 512 ) ( -1378 -275 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1182 -275 0 ) ( -1182 -275 512 ) ( -1280 -256 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1099 -331 0 ) ( -1099 -331 512 ) ( -1182 -275 0 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1043 -414 0 ) ( -1043 -414 512 ) ( -1099 -331 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"( -1024 -512 0 ) ( -1024 -512 512 ) ( -1043 -414 0 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
					"}",
					"}"
				};

				var sb = new StringBuilder();
				foreach (var s in raw)
				{
					sb.Append(s);
				}

				var map = new QuakeMap();
				map.Raw = sb.ToString();
				map.Parse();

				Assert.That(map.Blocks.Count, Is.EqualTo(1));

				var worldspawn = map.MapObjects[0] as QuakeMapObject;

				Assert.That(worldspawn.Block.KeyVals["classname"][0], Is.EqualTo("worldspawn"));
				Assert.That(worldspawn.Block.KeyVals.Count, Is.EqualTo(27));
				Assert.That(worldspawn.Children.Count, Is.EqualTo(4));

				var sideCount = new int[] { 6, 5, 6, 18 };
				for (var i = 0; i < sideCount.Length; i++)
				{
					var solid = worldspawn.Children[i] as QuakeMapObject;
					var block = solid.Block as QuakeBlock;

					Assert.That(block.Sides.Count, Is.EqualTo(sideCount[i]));
				}
			}

			[TestCase]
			public void OpenBraceInTextureName()
			{
				var raw = new List<string>()
				{
					"{",
					"\"classname\" \"worldspawn\"",
					"\"light\" \"0\"",
					"\"sounds\" \"1\"",
					"\"worldtype\" \"0\"",
					"\"origin\" \"0 0 0\"",
					"\"mapversion\" \"220\"",
					"\"wad\" \"D:\\Projects\\Games\\Maps\\Quake\\common\\wads\\quake.wad\"",
					"}",
					"{",
					"\"classname\" \"func_detail\"",
					"\"_phong\" \"1\"",
					"{",
					"( 0 48 120 ) ( 0 48 56 ) ( 0 -48 120 ) {SOMETRANSPARENTTEXTURE [ 0 1 0 1520 ] [ 0 0 -1 -1632 ] 0 1 1",
					"( -16 48 120 ) ( -16 -48 120 ) ( -16 48 56 ) {SOMETRANSPARENTTEXTURE [ 0 1 0 1520 ] [ 0 0 -1 -1632 ] 0 1 1",
					"( -16 48 120 ) ( -16 48 56 ) ( 0 48 120 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 -1632 ] 0 1 1",
					"( -16 -48 120 ) ( -16 48 120 ) ( 0 -48 120 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 -1520 ] 0 1 1",
					"( -16 48 56 ) ( -16 -48 120 ) ( 0 48 56 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 -1520 ] 0 1 1",
					"}",
					"}"
				};

				var sb = new StringBuilder();
				foreach (var s in raw)
				{
					sb.Append(s);
				}

				var map = new QuakeMap();
				map.Raw = sb.ToString();
				map.Parse();

				Assert.That(map.Blocks.Count, Is.EqualTo(2));

				var worldspawn = map.MapObjects[0] as QuakeMapObject;

				Assert.That(worldspawn.Block.KeyVals.Count, Is.EqualTo(7));
				Assert.That(worldspawn.Block.KeyVals["classname"][0], Is.EqualTo("worldspawn"));
				Assert.That(worldspawn.Children.Count, Is.EqualTo(0));

				var entity = map.MapObjects[1] as QuakeMapObject;

				Assert.That(entity.Block.KeyVals.Count, Is.EqualTo(2));
				Assert.That(entity.Block.KeyVals["classname"][0], Is.EqualTo("func_detail"));

				var entityBlock = entity.Block.Children[0] as QuakeBlock;

				Assert.That(entityBlock.Sides.Count, Is.EqualTo(5));

				var side = new QuakeSide(entityBlock.Sides[0]);

				Assert.That(side.TextureName, Is.EqualTo("{SOMETRANSPARENTTEXTURE"));
			}

			[TestCase]
			public void WorldspawnNotFirst()
			{
				var raw = new List<string>()
				{
					"{",
					"\"classname\" \"func_detail\"",
					"\"_phong\" \"1\"",
					"{",
					"( 0 48 120 ) ( 0 48 56 ) ( 0 -48 120 ) {SOMETRANSPARENTTEXTURE [ 0 1 0 1520 ] [ 0 0 -1 -1632 ] 0 1 1",
					"( -16 48 120 ) ( -16 -48 120 ) ( -16 48 56 ) {SOMETRANSPARENTTEXTURE [ 0 1 0 1520 ] [ 0 0 -1 -1632 ] 0 1 1",
					"( -16 48 120 ) ( -16 48 56 ) ( 0 48 120 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 -1632 ] 0 1 1",
					"( -16 -48 120 ) ( -16 48 120 ) ( 0 -48 120 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 -1520 ] 0 1 1",
					"( -16 48 56 ) ( -16 -48 120 ) ( 0 48 56 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 -1520 ] 0 1 1",
					"}",
					"}",
					"{",
					"\"classname\" \"worldspawn\"",
					"\"light\" \"0\"",
					"\"sounds\" \"1\"",
					"\"worldtype\" \"0\"",
					"\"origin\" \"0 0 0\"",
					"\"mapversion\" \"220\"",
					"\"wad\" \"D:\\Projects\\Games\\Maps\\Quake\\common\\wads\\quake.wad\"",
					"}"
				};

				var sb = new StringBuilder();
				foreach (var s in raw)
				{
					sb.Append(s);
				}

				var map = new QuakeMap();
				map.Raw = sb.ToString();
				map.Parse();

				Assert.That(map.Blocks.Count, Is.EqualTo(2));

				var entity = map.MapObjects[0] as QuakeMapObject;

				Assert.That(entity.Block.KeyVals.Count, Is.EqualTo(2));
				Assert.That(entity.Block.KeyVals["classname"][0], Is.EqualTo("func_detail"));

				var entityBlock = entity.Block.Children[0] as QuakeBlock;

				Assert.That(entityBlock.Sides.Count, Is.EqualTo(5));

				var side = new QuakeSide(entityBlock.Sides[0]);

				var worldspawn = map.MapObjects[1] as QuakeMapObject;

				Assert.That(worldspawn.Block.KeyVals.Count, Is.EqualTo(7));
				Assert.That(worldspawn.Block.KeyVals["classname"][0], Is.EqualTo("worldspawn"));
				Assert.That(worldspawn.Children.Count, Is.EqualTo(0));

				Assert.That(side.TextureName, Is.EqualTo("{SOMETRANSPARENTTEXTURE"));
			}
		}
	}
}
