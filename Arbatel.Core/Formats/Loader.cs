using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbatel.Formats.Quake;

namespace Arbatel.Formats
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

		public static TextureDictionary LoadTextureDictionary(string fileName, Palette palette)
		{
			TextureDictionary textures;

			bool isWad2 = Path.GetExtension(fileName).ToLower() == ".wad";

			if (isWad2)
			{
				textures = new Wad2(fileName, palette);
			}
			else
			{
				textures = new TextureDictionary();
			}

			return textures;
		}
		public static TextureDictionary LoadTextureDictionary(string fileName, string palettePath)
		{
			var paletteStream = new FileStream(palettePath, FileMode.Open, FileAccess.Read);

			// TODO: Accommodate more than just Quake palettes.
			var palette = new Palette().LoadQuakePalette(paletteStream);

			return LoadTextureDictionary(fileName, palette);
		}
	}
}
