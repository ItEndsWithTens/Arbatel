using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats.Quake;

namespace Temblor.Formats
{
	public static class Loader
	{
		public static DefinitionDictionary LoadDefinitionDictionary(string fileName)
		{
			DefinitionDictionary definitions;

			// TODO: This is hardly, shall we say, robust. Find a better way.
			bool isQuake = Path.GetExtension(fileName).ToLower() == ".fgd";

			if (isQuake)
			{
				definitions = new QuakeFgd(fileName);
			}
			else
			{
				definitions = new DefinitionDictionary(fileName);
			}

			return definitions;
		}

		public static TextureDictionary LoadTextureDictionary(string fileName)
		{
			TextureDictionary textures;

			bool isWad2 = Path.GetExtension(fileName).ToLower() == ".wad";

			if (isWad2)
			{
				// TODO: Finalize this!
				var palettePath = "D:/Development/Temblor/res/palette-quake.lmp";
				var palette = new Palette().LoadQuakePalette(palettePath);

				textures = new Wad2(fileName, palette);
			}
			else
			{
				textures = new TextureDictionary();
			}

			return textures;
		}
	}
}
