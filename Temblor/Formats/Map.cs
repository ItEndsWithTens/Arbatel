using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Graphics;

namespace Temblor
{
	public class Map
	{
		public string OpenDelimiter = "{";
		public string CloseDelimiter = "}";

		public string Raw;

		public List<Block> Blocks = new List<Block>();

		public List<Renderable> Renderables = new List<Renderable>();

		public Map()
		{

		}
		public Map(Stream stream)
		{
			using (StreamReader sr = new StreamReader(stream))
			{
				Raw = sr.ReadToEnd();
			}
		}

		virtual public void Parse() { }
	}
}
