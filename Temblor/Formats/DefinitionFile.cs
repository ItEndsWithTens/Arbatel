using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class DefinitionFile
	{
		public List<string> Raw;

		public List<EntityDefinition> Entities;

		public DefinitionFile()
		{
			Entities = new List<EntityDefinition>();
		}
		public DefinitionFile(Stream stream) : this()
		{
			using (var sr = new StreamReader(stream))
			{
				Parse(sr);
			}
		}

		virtual public void Parse(StreamReader sr)
		{
			Raw = Preprocess(sr);
		}

		/// <summary>
		/// Prepare an input definition file for subsequent parsing.
		/// </summary>
		/// <param name="sr"></param>
		/// <returns></returns>
		virtual public List<string> Preprocess(StreamReader sr)
		{
			var list = new List<string>();

			while (!sr.EndOfStream)
			{
				list.Add(sr.ReadLine());
			}

			return list;
		}
	}
}
