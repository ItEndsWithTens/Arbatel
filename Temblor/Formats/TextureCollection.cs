using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class TextureCollection
	{
		public Dictionary<string, Texture> Textures;

		public TextureCollection()
		{
			Textures = new Dictionary<string, Texture>();
		}
	}
}
