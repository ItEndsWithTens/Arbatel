using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class TextureDictionary : Dictionary<string, Texture>
	{
		public TextureDictionary()
		{
		}
		public TextureDictionary(TextureDictionary collection) : base(collection)
		{
		}
	}
}
