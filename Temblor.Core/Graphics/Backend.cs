using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace Temblor.Graphics
{
	public class Backend
	{
		public Dictionary<string, int> Textures { get; } = new Dictionary<string, int>();

		virtual public void InitTextures(TextureDictionary dictionary)
		{
		}
	}
}
