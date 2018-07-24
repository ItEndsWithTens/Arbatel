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

		public string Raw;

		public List<MapObject> MapObjects;

		public Map()
		{
			MapObjects = new List<MapObject>();
		}
		public Map(Stream stream) : this()
		{
			using (StreamReader sr = new StreamReader(stream))
			{
				Raw = sr.ReadToEnd();
			}
		}

		virtual public void Parse() { }
	}
}
