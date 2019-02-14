using Arbatel.Formats;
using Arbatel.Formats.Quake;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace ArbatelTest.Core.NeedsEto.Formats
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
					"( -256 512 512 ) ( -256 512 0 ) ( -256 0 512 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1" +
					"( -768 0 512 ) ( -768 0 0 ) ( -768 512 512 ) TRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1" +
					"( -256 0 512 ) ( -256 0 0 ) ( -768 0 512 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1" +
					"( -768 512 512 ) ( -768 512 0 ) ( -256 512 512 ) TRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1" +
					"( -768 512 0 ) ( -768 0 0 ) ( -256 512 0 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1" +
					"( -256 0 512 ) ( -768 0 512 ) ( -256 512 512 ) TRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
					"}"
				};

				var block = new QuakeBlock(raw, 0, new DefinitionDictionary());
			}

			[TestCase]
			public void SuperfluousFace()
			{
				// In this case, the side with exit02_2 as its texture is an
				// invalid, extra side, which doesn't produce an actual polygon.
				var raw = new List<string>()
				{
					"{",
					"\"_phong\" \"1\"" +
					"\"classname\" \"func_detail\"",
					"{",
					"( -2272 1104 832 ) ( -2256 1104 832 ) ( -2272 1024 832 ) ceiling5 0 32 0 1.000000 1.000000" +
					"( -2272 1104 832 ) ( -2272 1040 832 ) ( -2264 1040 800 ) city2_5 0 32 0 1.000000 1.000000" +
					"( -2240 1024 832 ) ( -2240 1104 832 ) ( -2240 1104 800 ) comp1_6 0 32 0 1.000000 1.000000" +
					"( -2272 1040 832 ) ( -2256 1024 832 ) ( -2248 1024 800 ) cop1_1 8 32 0 0.700000 1.000000" +
					"( -2256 1024 832 ) ( -2240 1024 832 ) ( -2240 1024 800 ) dr05_2 0 32 0 -1.000000 1.000000" +
					"( -2264 1104 688 ) ( -2264 1080 688 ) ( -2208 1104 688 ) exit02_2 0 32 0 1.000000 1.000000" +
					"( -2240 1104 832 ) ( -2272 1104 832 ) ( -2264 1104 800 ) floor01_5 0 32 0 1.000000 1.000000",
					"}",
					"}"
				};

				var rawFgd = new List<string>()
				{
					"@SolidClass color(128 128 230)base(ModelLight) = func_detail : \"Detail brush. Ignored by vis so can speed up compile times consideratbly. Also allows you to set new compiler lighting options on brushes. DOES NOT SEAL MAPS FROM VOID\" []",
					"",
					"@baseclass = ModelLight",
					"[",
					"	_minlight(integer) : \"Min light\" :  : \"Set the minimum light level for any surface of the brush model. Default 0\"",
					"	_mincolor(color255) : \"Min light color R G B\" : \"255 255 255\" : \"Specify red(r), green(g) and blue(b) components for the colour of the minlight. RGB component values are between 0 and 255 (between 0 and 1 is also accepted). Default is white light (255 255 255)\"",
					"	_shadow(integer) : \"Shadows\" :  : \"If n is 1, this model will cast shadows on other models and itself (i.e. '_shadow' implies '_shadowself'). Note that this doesn’t magically give Quake dynamic lighting powers, so the shadows will not move if the model moves. Func_detail ONLY - If set to -1, light will pass through this brush model. Default 0\"",
					"	_shadowself(integer) : \"Self Shadow\" :  : \"If n is 1, this model will cast shadows on itself if one part of the model blocks the light from another model surface. This can be a better compromise for moving models than full shadowing. Default 0\"",
					"	_dirt(integer) : \"Dirt mapping (override)\" :  : \"For brush models, -1 prevents dirtmapping on the brush model. Useful it the bmodel touches or sticks into the world, and you want to those ares from turning black. Default 0\"",
					"	_phong(choices) : \"Enable Phong shading\" : 0 =",
					"	[",
					"		0: \"No\"",
					"		1: \"Yes\"",
					"	]",
					"	_phong_angle(integer) : \"Phong shading angle\" :  : \"Enables phong shading on faces of this model with a custom angle. Adjacent faces with normals this many degrees apart (or less) will be smoothed. Consider setting '_anglescale' to '1' on lights or worldspawn to make the effect of phong shading more visible. Use the '-phongdebug' command-line flag to save the interpolated normals to the lightmap for previewing (use 'r_lightmap 1' or 'gl_lightmaps 1' in your engine to preview.)\"",
					"]"
				};

				var stream = new MemoryStream();
				var sw = new StreamWriter(stream);
				foreach (var line in rawFgd)
				{
					sw.WriteLine(line);
				}
				sw.Flush();
				stream.Position = 0;

				var fgd = new QuakeFgd(stream);

				// The assertions below are really just icing on the cake; the
				// important part is successfully instantiating a QuakeMapObject
				// without throwing any exceptions.
				var qmo = new QuakeMapObject(new QuakeBlock(raw, 0, fgd), fgd);

				Assert.That(qmo.KeyVals.Count, Is.EqualTo(2));

				Assert.That(qmo.Renderables[0].Vertices.Count, Is.EqualTo(24));

				var indexCount = 0;
				foreach (var polygon in qmo.Renderables[0].Polygons)
				{
					indexCount += polygon.Indices.Count;
				}

				Assert.That(indexCount, Is.EqualTo(24));
			}

			public class OpenBraceInTextureName
			{
				[TestCase]
				public void ParseSingleBlock()
				{
					// Test block lifted from ad_sepulcher.
					List<string> raw = new List<string>()
					{
						"{",
						"\"_minlight\" \"25\""+
						"\"classname\" \"func_detail_illusionary\"" +
						"\"_phong\" \"1\"" +
						"\"spawnflags\" \"32\"" +
						"\"_shadow\" \"1\"",
						"{",
						"( -1232 -1076 32 ) ( -1232 -1074 32 ) ( -1232 -1074 -104 ) skip 0 0 0 1.000000 1.000000" +
						"( -1280 -1084 -104 ) ( -1280 -1082 -104 ) ( -1280 -1082 32 ) skip 0 0 0 1.000000 1.000000" +
						"( -1280 -1082 0 ) ( -1280 -1082 -136 ) ( -1232 -1074 -136 ) {vinehang2b -104 64 0 1.000000 1.000000" +
						"( -1280 -1084 -136 ) ( -1280 -1084 0 ) ( -1232 -1076 0 ) {vinehang2b -104 64 0 1.000000 1.000000" +
						"( -1280 -1084 0 ) ( -1280 -1082 0 ) ( -1232 -1074 0 ) skip 0 0 0 1.000000 1.000000" +
						"( -1280 -1082 -136 ) ( -1280 -1084 -136 ) ( -1232 -1076 -136 ) skip 0 0 0 1.000000 1.000000",
						"}",
						"{",
						"( -1184 -1088 -136 ) ( -1184 -1086 -136 ) ( -1232 -1074 -136 ) skip 0 0 0 1.000000 1.000000" +
						"( -1184 -1086 0 ) ( -1184 -1088 0 ) ( -1232 -1076 0 ) skip 0 0 0 1.000000 1.000000" +
						"( -1184 -1088 0 ) ( -1184 -1088 -136 ) ( -1232 -1076 -136 ) {vinehang2b -104 64 0 1.000000 1.000000" +
						"( -1184 -1086 -136 ) ( -1184 -1086 0 ) ( -1232 -1074 0 ) {vinehang2b -104 64 0 1.000000 1.000000" +
						"( -1232 -1076 -104 ) ( -1232 -1074 -104 ) ( -1232 -1074 32 ) skip 0 0 0 1.000000 1.000000" +
						"( -1184 -1088 32 ) ( -1184 -1086 32 ) ( -1184 -1086 -104 ) skip 0 0 0 1.000000 1.000000",
						"}",
						"}"
					};

					var block = new QuakeBlock(raw, 1, new DefinitionDictionary());

					Assert.That(block.KeyVals.Count, Is.EqualTo(5));

					var child = block.Children[0] as QuakeBlock;

					Assert.That(child.Solids[0].Sides[2].TextureName, Is.EqualTo("{vinehang2b"));
					Assert.That(child.Solids[0].Sides[3].TextureName, Is.EqualTo("{vinehang2b"));
				}
			}
		}
	}
}
