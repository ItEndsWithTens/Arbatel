using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class DefinitionCollection
	{
		public List<string> Raw;

		public List<Definition> Definitions;

		public DefinitionCollection()
		{
			Definitions = new List<Definition>();
		}
		public DefinitionCollection(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read))
		{
		}
		public DefinitionCollection(Stream stream) : this()
		{
			using (var sr = new StreamReader(stream))
			{
				Parse(sr);
			}
		}

		public Definition this[string s]
		{
			get
			{
				Definition entity = null;

				for (var i = 0; i < Definitions.Count; i++)
				{
					if (Definitions[i].ClassName == s)
					{
						entity = Definitions[i];
						break;
					}
				}

				return entity;
			}

			set
			{
				for (var i = 0; i < Definitions.Count; i++)
				{
					Definition entity = Definitions[i];

					if (entity.ClassName == s)
					{
						Definitions[i] = value;
						break;
					}
				}
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
