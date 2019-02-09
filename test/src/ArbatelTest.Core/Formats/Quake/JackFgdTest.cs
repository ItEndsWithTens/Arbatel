using Arbatel.Formats;
using Arbatel.Graphics;
using NUnit.Framework;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArbatelTest.Core.Formats.Quake
{
	[TestFixture]
	public class JackFgdTest
	{
		public QuakeFgd Fgd;

		[SetUp]
		public void SetUp()
		{
			var list = new List<string>()
			{
				"@baseclass = Appearflags [",
				"	spawnflags(Flags) =",
				"	[",
				"		256 : \"Not in Easy\" : 0 : \"Will not spawn for EASY skill\"",
				"		512 : \"Not in Normal\" : 0",
				"		1024 : \"Not in Hard\" : 0",
				"		2048 : \"Not in Deathmatch\" : 0 : \"Will not spawn for DEATHMATCH skill\"",
				"	]",
				"]",
				"",
				"@baseclass = Target [ target(target_destination) : \"Target\" ]",
				"",
				"@baseclass = Item",
				"[",
				"	message(string) : \"Message\"",
				"]",
				"",
				"@baseclass size(0 0 0, 32 32 32) color(80 0 200) base(Item, Appearflags, Target) = Ammo",
				"[",
				"	spawnflags(flags) = ",
				"	[",
				"		1 : \"Large box\" : 0",
				"	]",
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
				"]",
				"",
				"@BaseClass = FakeThingToTestChoices",
				"[",
				"	_phony(choices) : \"I am a banana\" = [",
				"		0: \"My\"",
				"		1 :\"spoon\"",
				"	]",
				"]",
				"",
				"@PointClass = ambient_custom_rain : \"Play a rain sound. Always starts off and must be triggered. NOTE - If you re-trigger(ex button) this entity off it will not restart henceforth.\"[",
				"	count(choices) : \"Type of Rain\" = [",
				"	   0 : \"Default\"",
				"",
				"	   1 : \"Fast Dripping\"",
				"",
				"	   2 : \"Dowmpour(def)\"",
				"	]",
				"	volume(choices) : \"Volume\" :  : \"Volume of sound  (0.1->1,(def=1).\" = [",
				"		0.1 : \"Barely Audible\"",
				"		0.2 : \"0.2\"",
				"		0.3 : \"0.3\"",
				"		0.4 : \"0.4\"",
				"		0.5 : \"0.5\"",
				"		0.6 : \"0.6\"",
				"		0.7 : \"0.7\"",
				"		0.8 : \"0.8\"",
				"		0.9 : \"0.9\"",
				"		1.0 : \"Full Volume(def)\"",
				"	]",
				"]",
				"",
				"@SolidClass = trigger_touchsound : \"Trigger Touch Sounds\"[",
				"	spawnflags(flags) = [",
				"		4 : \"World Geo\"",
				"		8 : \"Drain\"",
				"	]",
				"	message(choices) : \"Sounds\" = [",
				"		1 : \"Water (DEF)\"",
				"		2 : \"Slime\"",
				"		3 : \"Lava\"",
				"		4 : \"Silent\"",
				"		5 : \"Custom\"",
				"	]",
				"	noise(sound) : \"Custom Touch Sound\"",
				"	noise1(sound) : \"Custom Exit Sound\"",
				"	noise2(sound) : \"Custom Drain Sound\"",
				"	speed(integer) : \"Drain Time\"",
				"	yaw_speed(integer) : \"Drain Movement\"",
				"	super_time(integer) : \"Time delay playing drain sound\"",
				"	water_alpha(integer) : \"Liquid Alpha\"",
				"]"
			};

			var stream = new MemoryStream();
			var sw = new StreamWriter(stream);
			sw.Write(String.Join("\n", list));
			sw.Flush();
			stream.Position = 0;

			Fgd = new QuakeFgd(stream);

			sw.Close();
		}

		[TestCase]
		public void TopLevelProperties()
		{
			Definition def = Fgd["Appearflags"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(0));
				Assert.That(def.ClassName, Is.EqualTo("Appearflags"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Base));
				Assert.That(def.Color, Is.EqualTo(new Color4(0, 0, 0, 0)));
				Assert.That(def.Description, Is.Null);
				Assert.That(def.Flags.Count, Is.EqualTo(4));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(1));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string>()));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.None));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.None));
				Assert.That(def.Size, Is.Null);
			});

			def = Fgd["Target"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(0));
				Assert.That(def.ClassName, Is.EqualTo("Target"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Base));
				Assert.That(def.Color, Is.EqualTo(new Color4(0, 0, 0, 0)));
				Assert.That(def.Description, Is.Null);
				Assert.That(def.Flags.Count, Is.EqualTo(0));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(2));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string>()));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.None));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.None));
				Assert.That(def.Size, Is.Null);
			});

			def = Fgd["Item"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(0));
				Assert.That(def.ClassName, Is.EqualTo("Item"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Base));
				Assert.That(def.Color, Is.EqualTo(new Color4(0, 0, 0, 0)));
				Assert.That(def.Description, Is.Null);
				Assert.That(def.Flags.Count, Is.EqualTo(0));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(2));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string>()));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.None));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.None));
				Assert.That(def.Size, Is.Null);
			});

			def = Fgd["Ammo"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(3));
				Assert.That(def.BaseNames.Contains("Item"));
				Assert.That(def.BaseNames.Contains("Appearflags"));
				Assert.That(def.BaseNames.Contains("Target"));
				Assert.That(def.ClassName, Is.EqualTo("Ammo"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Base));
				Assert.That(def.Color, Is.EqualTo(new Color4(80, 0, 200, 255)));
				Assert.That(def.Description, Is.Null);
				Assert.That(def.Flags.Count, Is.EqualTo(5));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(3));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string> { { RenderableSource.Size, String.Empty } }));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.None));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.None));
				Assert.That(def.Size, Is.EqualTo(new Aabb(new Vector3(0), new Vector3(32))));
			});

			def = Fgd["ModelLight"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(0));
				Assert.That(def.ClassName, Is.EqualTo("ModelLight"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Base));
				Assert.That(def.Color, Is.EqualTo(new Color4(0, 0, 0, 0)));
				Assert.That(def.Description, Is.Null);
				Assert.That(def.Flags.Count, Is.EqualTo(0));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(8));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string>()));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.None));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.None));
				Assert.That(def.Size, Is.Null);
			});

			def = Fgd["func_detail"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(1));
				Assert.That(def.BaseNames[0], Is.EqualTo("ModelLight"));
				Assert.That(def.ClassName, Is.EqualTo("func_detail"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Solid));
				Assert.That(def.Color, Is.EqualTo(new Color4(128, 128, 230, 255)));
				Assert.That(def.Description, Is.EqualTo("Detail brush. Ignored by vis so can speed up compile times consideratbly. Also allows you to set new compiler lighting options on brushes. DOES NOT SEAL MAPS FROM VOID"));
				Assert.That(def.Flags.Count, Is.EqualTo(0));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(8));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string> { { RenderableSource.Solids, String.Empty } }));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.All));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.All));
				Assert.That(def.Size, Is.Null);
			});

			def = Fgd["FakeThingToTestChoices"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(0));
				Assert.That(def.ClassName, Is.EqualTo("FakeThingToTestChoices"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Base));
				Assert.That(def.Color, Is.EqualTo(new Color4(0, 0, 0, 0)));
				Assert.That(def.Description, Is.Null);
				Assert.That(def.Flags.Count, Is.EqualTo(0));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(2));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string>()));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.None));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.None));
				Assert.That(def.Size, Is.Null);
			});

			def = Fgd["trigger_touchsound"];
			Assert.Multiple(() =>
			{
				Assert.That(def.BaseNames.Count, Is.EqualTo(0));
				Assert.That(def.ClassName, Is.EqualTo("trigger_touchsound"));
				Assert.That(def.ClassType, Is.EqualTo(ClassType.Solid));
				Assert.That(def.Color, Is.EqualTo(new Color4(0, 0, 0, 0)));
				Assert.That(def.Description, Is.EqualTo("Trigger Touch Sounds"));
				Assert.That(def.Flags.Count, Is.EqualTo(2));
				Assert.That(def.KeyValsTemplate.Count, Is.EqualTo(9));
				Assert.That(def.Offset, Is.EqualTo(new Vector3(0)));
				Assert.That(def.RenderableSources, Is.EqualTo(new Dictionary<RenderableSource, string> { { RenderableSource.Solids, String.Empty } }));
				Assert.That(def.RenderableTransformability, Is.EqualTo(Transformability.All));
				Assert.That(def.Saveability, Is.EqualTo(Saveability.All));
				Assert.That(def.Size, Is.Null);
			});
		}

		[TestCase]
		public void KeyValsTemplate()
		{
			Dictionary<string, Option> keyVals = Fgd["func_detail"].KeyValsTemplate;

			Option minlight = keyVals["_minlight"];
			Assert.Multiple(() =>
			{
				Assert.That(minlight.Choices, Is.Empty);
				Assert.That(minlight.Default, Is.Null);
				Assert.That(minlight.Description, Is.EqualTo("Min light"));
				Assert.That(minlight.Remarks, Is.EqualTo("Set the minimum light level for any surface of the brush model. Default 0"));
				Assert.That(minlight.TransformType, Is.EqualTo(TransformType.None));
				Assert.That(minlight.Type, Is.EqualTo("integer"));
				Assert.That(minlight.Value, Is.Null);
			});

			Option mincolor = keyVals["_mincolor"];
			Assert.Multiple(() =>
			{
				Assert.That(mincolor.Choices, Is.Empty);
				Assert.That(mincolor.Default, Is.EqualTo("255 255 255"));
				Assert.That(mincolor.Description, Is.EqualTo("Min light color R G B"));
				Assert.That(mincolor.Remarks, Is.EqualTo("Specify red(r), green(g) and blue(b) components for the colour of the minlight. RGB component values are between 0 and 255 (between 0 and 1 is also accepted). Default is white light (255 255 255)"));
				Assert.That(mincolor.TransformType, Is.EqualTo(TransformType.None));
				Assert.That(mincolor.Type, Is.EqualTo("color255"));
				Assert.That(mincolor.Value, Is.Null);
			});
		}

		[TestCase]
		public void Spawnflags()
		{
			Dictionary<string, Spawnflag> flags = Fgd["Ammo"].Flags;

			Assert.Multiple(() =>
			{
				Assert.That(flags.Count, Is.EqualTo(5));
				Assert.That(flags["1"].Default, Is.EqualTo("0"));
				Assert.That(flags["256"].Default, Is.EqualTo("0"));
				Assert.That(flags["512"].Default, Is.EqualTo("0"));
				Assert.That(flags["1024"].Default, Is.EqualTo("0"));
				Assert.That(flags["2048"].Default, Is.EqualTo("0"));
				Assert.That(flags["1"].Description, Is.EqualTo("Large box"));
				Assert.That(flags["256"].Description, Is.EqualTo("Not in Easy"));
				Assert.That(flags["512"].Description, Is.EqualTo("Not in Normal"));
				Assert.That(flags["1024"].Description, Is.EqualTo("Not in Hard"));
				Assert.That(flags["2048"].Description, Is.EqualTo("Not in Deathmatch"));
			});
		}

		[TestCase]
		public void Choices()
		{
			Assert.That(Fgd.ContainsKey("ModelLight"));
			Definition entity = Fgd["ModelLight"];
			Assert.That(entity.KeyValsTemplate.ContainsKey("_phong"));
			Option option = entity.KeyValsTemplate["_phong"];
			Assert.Multiple(() =>
			{
				Assert.That(option.Choices.Count, Is.EqualTo(2));
				Assert.That(option.Choices["0"], Is.EqualTo("No"));
				Assert.That(option.Choices["1"], Is.EqualTo("Yes"));
				Assert.That(option.Default, Is.EqualTo("0"));
				Assert.That(option.Description, Is.EqualTo("Enable Phong shading"));
				Assert.That(option.Remarks, Is.Null);
				Assert.That(option.TransformType, Is.EqualTo(TransformType.None));
				Assert.That(option.Type, Is.EqualTo("choices"));
				Assert.That(option.Value, Is.Null);
			});

			Assert.That(Fgd.ContainsKey("FakeThingToTestChoices"));
			entity = Fgd["FakeThingToTestChoices"];
			Assert.That(entity.KeyValsTemplate.ContainsKey("_phony"));
			option = entity.KeyValsTemplate["_phony"];
			Assert.Multiple(() =>
			{
				Assert.That(option.Choices.Count, Is.EqualTo(2));
				Assert.That(option.Choices["0"], Is.EqualTo("My"));
				Assert.That(option.Choices["1"], Is.EqualTo("spoon"));
				Assert.That(option.Default, Is.Null);
				Assert.That(option.Description, Is.EqualTo("I am a banana"));
				Assert.That(option.Remarks, Is.Null);
				Assert.That(option.TransformType, Is.EqualTo(TransformType.None));
				Assert.That(option.Type, Is.EqualTo("choices"));
				Assert.That(option.Value, Is.Null);
			});

			Assert.That(Fgd.ContainsKey("ambient_custom_rain"));
			entity = Fgd["ambient_custom_rain"];
			Assert.That(entity.KeyValsTemplate.ContainsKey("count"));
			option = entity.KeyValsTemplate["count"];
			Assert.Multiple(() =>
			{
				Assert.That(option.Choices.Count, Is.EqualTo(3));
				Assert.That(option.Choices["0"], Is.EqualTo("Default"));
				Assert.That(option.Choices["1"], Is.EqualTo("Fast Dripping"));
				Assert.That(option.Choices["2"], Is.EqualTo("Dowmpour(def)"));
				Assert.That(option.Default, Is.Null);
				Assert.That(option.Description, Is.EqualTo("Type of Rain"));
				Assert.That(option.Remarks, Is.Null);
				Assert.That(option.TransformType, Is.EqualTo(TransformType.None));
				Assert.That(option.Type, Is.EqualTo("choices"));
				Assert.That(option.Value, Is.Null);
			});

			Assert.That(entity.KeyValsTemplate.ContainsKey("volume"));
			option = entity.KeyValsTemplate["volume"];
			Assert.Multiple(() =>
			{
				Assert.That(option.Choices.Count, Is.EqualTo(10));
				Assert.That(option.Choices["0.1"], Is.EqualTo("Barely Audible"));
				Assert.That(option.Choices["0.2"], Is.EqualTo("0.2"));
				Assert.That(option.Choices["0.3"], Is.EqualTo("0.3"));
				Assert.That(option.Choices["0.4"], Is.EqualTo("0.4"));
				Assert.That(option.Choices["0.5"], Is.EqualTo("0.5"));
				Assert.That(option.Choices["0.6"], Is.EqualTo("0.6"));
				Assert.That(option.Choices["0.7"], Is.EqualTo("0.7"));
				Assert.That(option.Choices["0.8"], Is.EqualTo("0.8"));
				Assert.That(option.Choices["0.9"], Is.EqualTo("0.9"));
				Assert.That(option.Choices["1.0"], Is.EqualTo("Full Volume(def)"));
				Assert.That(option.Default, Is.Null);
				Assert.That(option.Description, Is.EqualTo("Volume"));
				Assert.That(option.Remarks, Is.EqualTo("Volume of sound  (0.1->1,(def=1)."));
				Assert.That(option.TransformType, Is.EqualTo(TransformType.None));
				Assert.That(option.Type, Is.EqualTo("choices"));
				Assert.That(option.Value, Is.Null);
			});
		}
	}
}
