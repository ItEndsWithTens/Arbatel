using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Arbatel.Graphics;

namespace Arbatel.Formats
{
	public static class TextureDictionaryExtensions
	{
		public static TextureDictionary Stack(this List<TextureDictionary> dictionaries)
		{
			var stacked = new TextureDictionary();

			foreach (var dictionary in dictionaries)
			{
				foreach (var texture in dictionary.Values)
				{
					if (stacked.ContainsKey(texture.Name))
					{
						stacked[texture.Name] = texture;
					}
					else
					{
						stacked.Add(texture.Name, texture);
					}
				}

				foreach (var translucent in dictionary.Translucents)
				{
					if (!stacked.Translucents.Contains(translucent))
					{
						stacked.Translucents.Add(translucent);
					}
				}
			}

			return stacked;
		}
	}

	public class TextureDictionary : Dictionary<string, Texture>
	{
		/// <summary>
		/// The names of any translucent or transparent textures in this dictionary.
		/// </summary>
		public List<string> Translucents { get; protected set; } = new List<string>();

		public TextureDictionary()
		{
		}
		public TextureDictionary(TextureDictionary collection) : base(collection)
		{
		}
	}
}
