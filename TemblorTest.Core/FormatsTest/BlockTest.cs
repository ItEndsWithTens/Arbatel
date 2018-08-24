using Eto.Forms;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Formats.Quake;

namespace TemblorTest.Core.FormatsTest
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
					"( -256 512 512 ) ( -256 512 0 ) ( -256 0 512 ) TRIGGER [ 0 1 0 0 ] [ 0 0 - 1 0 ] 0 1 1" +
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

				Assert.That(qmo.Renderables[0].Vertices.Count, Is.EqualTo(8));

				var indexCount = 0;
				foreach (var polygon in qmo.Renderables[0].Polygons)
				{
					indexCount += polygon.Indices.Count;
				}

				Assert.That(indexCount, Is.EqualTo(36));
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

	public class MapTest
	{
		[TestFixture]
		public class QuakeMapTest
		{
			List<string> RawFgd = new List<string>()
			{
				"@SolidClass = worldspawn : \"World entity\"",
				"[",
				"	message(string) : \"Level name\"",
				"	worldtype(choices) : \"Ambience\" : 0 =",
				"	[",
				"		0 : \"Medieval\"",
				"		1 : \"Runic (metal)\"",
				"		2 : \"Present (base)\"",
				"	]",
				"	sounds(integer) : \"CD track to play\" : 1",
				"	light(integer) : \"Ambient light\" : 0 : \"Set a global minimum light level of 'n' across the whole map. This is an easy way to eliminate completely dark areas of the level, however you may lose some contrast as a result, so use with care. Default 0\"",
				"	_sunlight(integer) : \"Sunlight\" : 0 : \"Set the brightness of the sunlight coming from an unseen sun in the sky. Sky brushes (or more accurately bsp leafs with sky contents) will emit sunlight at an angle specified by the _sun_mangle key. Default 0\"",
				"	_sun_mangle(string) : \"Sun mangle (Yaw pitch roll)\" : \"0 -90 0\" : \"Specifies the direction of sunlight using yaw(x), pitch(y) and roll(z) in degrees. Yaw specifies the angle around the Z-axis from 0 to 359 degrees and pitch specifies the angle from 90 (straight up) to -90 (straight down). Roll has no effect, so use any value (e.g. 0). Default is straight down (0 -90 0)\"",
				"	_sunlight_penumbra(integer) : \"Sunlight penumbra in degrees\" : 0 : \"Specifies the penumbra width, in degrees, of sunlight. Useful values are 3-4 for a gentle soft edge, or 10-20+ for more diffuse sunlight. Default is 0\"",
				"	_sunlight_color(color255) : \"Sunlight color R G B\" : \"255 255 255\" : \"Specify red(r), green(g) and blue(b) components for the colour of the sunlight. RGB component values are between 0 and 255 (between 0 and 1 is also accepted). Default is white light (255 255 255) \"",
				"	_sunlight2(integer) : \"Sunlight 2 brightness\" : 0 : \"Set the brightness of a large dome of lights positioned around the map (16K unit radius). Useful for simulating higly diffused light (e.g. cloudy skies) in outdoor areas. Default 0\"",
				"	_sunlight2_color(color255) : \"Sunlight 2 color R G B\" : \"255 255 255\" : \"Specifies the colour of _sunlight2, same format as _sunlight_color. Default is white light (255 255 255) \"",
				"	_sunlight3(integer) : \"Sunlight 3 brightness\" : 0 : \"Same as _sunlight2 but creates lights on the bottom hemisphere. Default 0\"",
				"	_sunlight3_color(color255) : \"Sunlight 3 color R G B\" : \"255 255 255\" : \"Specifies the colour of _sunlight3, same format as _sunlight_color. Default is white light (255 255 255)\"",
				"	_dist(integer) : \"Global light scale\" : 1 : \"Scales the fade distance of all lights by a factor of n. If n is more than 1 lights fade more quickly with distance and if n is less than 1, lights fade more slowly with distance and light reaches further\"",
				"	_range(float) : \"Global light range\" : \"0.5\" : \"Scales the brightness range of all lights without affecting their fade discance. Values of n more than 0.5 makes lights brighter and n less than 0.5 makes lights less bright. The same effect can be achieved on individual lights by adjusting both the 'light' and 'wait' attributes\"",
				"	_anglescale(float) : \"Light angle scale\" : \"0.5\" : \"Sets a scaling factor for how much influence the angle of incidence of sunlight on a surface has on the brightness of the surface. n must be between 0.0 and 1.0. Smaller values mean less attenuation, with zero meaning that angle of incidence has no effect at all on the brightness. Default 0.5\"",
				"	_dirt(integer) : \"Dirt mapping (AO)\" : -1 : \"1 enables dirtmapping (ambient occlusion) on all lights, borrowed from q3map2. This adds shadows to corners and crevices. You can override the global setting for specific lights with the _dirt light entity key or _sunlight_dirt, _sunlight2_dirt, and _minlight_dirt worldspawn keys. Default is no dirtmapping (-1)\"",
				"	_sunlight_dirt(integer) : \"Sunlight dirt\" : -1 : \"1 enables dirtmapping (ambient occlusion) on sunlight, -1 to disable (making it illuminate the dirtmapping shadows). Default is to use the value of '_dirt'\"",
				"	_sunlight2_dirt(integer) : \"Sublight 2 dirt\" : -1 : \"1 enables dirtmapping (ambient occlusion) on sunlight2, -1 to disable. Default is to use the value of '_dirt'\"",
				"	_minlight_dirt(integer) : \"Minlight dirt\" : -1 : \"1 enables dirtmapping (ambient occlusion) on minlight, -1 to disable. Default is to use the value of '_dirt'\"",
				"	_dirtmode(integer) : \"Dirt mode\" : 0 : \"Choose between ordered (0, default) and randomized (1) dirtmapping.\"",
				"	_dirtdepth(integer) : \"Dirt depth\" : 128 : \"Maximum depth of occlusion checking for dirtmapping, default 128.\"",
				"	_dirtscale(integer) : \"Dirt scale\" : 1 : \"Scale factor used in dirt calculations, default 1. Lower values (e.g. 0.5) make the dirt fainter, 2.0 would create much darker shadows\"",
				"	_dirtgain(integer) : \"Dirt gain\" : 1 : \"Exponent used in dirt calculation, default 1. Lower values (e.g. 0.5) make the shadows darker and stretch further away from corners\"",
				"	_gamma(integer) : \"Lightmap gamma\" : 1 : \"Adjust brightness of final lightmap. Default 1, >1 is brighter, <1 is darker\"",
				"]",
				"",
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

			DefinitionDictionary Fgd;

			[SetUp]
			public void SetUp()
			{
				var fgdStream = new MemoryStream();
				var fgdWriter = new StreamWriter(fgdStream);
				foreach (var line in RawFgd)
				{
					fgdWriter.WriteLine(line);
				}
				fgdWriter.Flush();
				fgdStream.Position = 0;

				Fgd = new QuakeFgd(fgdStream);
			}

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

				var mapStream = new MemoryStream();
				var mapWriter = new StreamWriter(mapStream);
				foreach (var line in raw)
				{
					mapWriter.WriteLine(line);
				}
				mapWriter.Flush();
				mapStream.Position = 0;

				var map = new QuakeMap(mapStream, Fgd);

				Assert.That(map.MapObjects.Count, Is.EqualTo(1));

				var worldspawn = map.MapObjects[0] as QuakeMapObject;

				Assert.That(worldspawn.KeyVals["classname"].ToString(), Is.EqualTo("worldspawn"));
				Assert.That(worldspawn.KeyVals.Count, Is.EqualTo(27));
				Assert.That(worldspawn.Renderables.Count, Is.EqualTo(4));

				var sideCount = new int[] { 6, 5, 6, 18 };
				for (var i = 0; i < sideCount.Length; i++)
				{
					var solid = worldspawn.Renderables[i];

					Assert.That(solid.Polygons.Count, Is.EqualTo(sideCount[i]));
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

				var mapStream = new MemoryStream();
				var mapWriter = new StreamWriter(mapStream);
				foreach (var line in raw)
				{
					mapWriter.WriteLine(line);
				}
				mapWriter.Flush();
				mapStream.Position = 0;

				var map = new QuakeMap(mapStream, Fgd);

				Assert.That(map.MapObjects.Count, Is.EqualTo(2));

				var worldspawn = map.MapObjects[0] as QuakeMapObject;

				Assert.That(worldspawn.KeyVals.Count, Is.EqualTo(7));
				Assert.That(worldspawn.KeyVals["classname"].ToString(), Is.EqualTo("worldspawn"));
				Assert.That(worldspawn.Children.Count, Is.EqualTo(0));

				var entity = map.MapObjects[1] as QuakeMapObject;

				Assert.That(entity.KeyVals.Count, Is.EqualTo(2));
				Assert.That(entity.KeyVals["classname"].ToString(), Is.EqualTo("func_detail"));

				Assert.That(entity.Renderables[0].Polygons.Count, Is.EqualTo(5));

				var polygon = entity.Renderables[0].Polygons[0];

				Assert.That(polygon.Texture.Name, Is.EqualTo("{sometransparenttexture"));
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

				var mapStream = new MemoryStream();
				var mapWriter = new StreamWriter(mapStream);
				foreach (var line in raw)
				{
					mapWriter.WriteLine(line);
				}
				mapWriter.Flush();
				mapStream.Position = 0;

				var map = new QuakeMap(mapStream, Fgd);

				Assert.That(map.MapObjects.Count, Is.EqualTo(2));

				var entity = map.MapObjects[0] as QuakeMapObject;

				Assert.That(entity.KeyVals.Count, Is.EqualTo(2));
				Assert.That(entity.KeyVals["classname"].ToString(), Is.EqualTo("func_detail"));

				Assert.That(entity.Renderables[0].Polygons.Count, Is.EqualTo(5));

				var worldspawn = map.MapObjects[1] as QuakeMapObject;

				Assert.That(worldspawn.KeyVals.Count, Is.EqualTo(7));
				Assert.That(worldspawn.KeyVals["classname"].ToString(), Is.EqualTo("worldspawn"));
				Assert.That(worldspawn.Children.Count, Is.EqualTo(0));

				var polygon = entity.Renderables[0].Polygons[0];

				Assert.That(polygon.Texture.Name, Is.EqualTo("{sometransparenttexture"));
			}
		}
	}
}
