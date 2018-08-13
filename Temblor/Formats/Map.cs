using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace Temblor.Formats
{
	public class Map
	{
		public string OpenDelimiter = "{";
		public string CloseDelimiter = "}";

		public DefinitionCollection Definitions;

		public List<MapObject> MapObjects;

		public List<TextureCollection> Textures;

		public Map()
		{
			MapObjects = new List<MapObject>();
		}
		public Map(string filename, DefinitionCollection definitions) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions)
		{
		}
		public Map(Stream stream, DefinitionCollection definitions) : this()
		{
			Definitions = definitions;

			using (StreamReader sr = new StreamReader(stream))
			{
				Parse(sr.ReadToEnd());
			}
		}

		virtual public void Parse(string raw)
		{
		}
	}
}
