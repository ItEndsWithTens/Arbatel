using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats.Quake
{
	public enum CollapseType
	{
		Position,
		Angle,
		Target
	}

	public static class QuakeMapExtensions
	{
		/// <summary>
		/// Update the Saveability of every MapObject in this map, setting
		/// instance entities to be Saveability.Children.
		/// </summary>
		/// <param name="map"></param>
		/// <returns>A copy of the input Map with its MapObjects' Saveability
		/// updated, in preparation for writing to disk as a string.</returns>
		public static QuakeMap Collapse(this QuakeMap map)
		{
			var collapsed = new QuakeMap(map);
			collapsed.MapObjects.Clear();

			int worldspawnIndex = map.MapObjects.FindIndex(o => o.Definition.ClassName == "worldspawn");

			MapObject worldspawn = map.MapObjects[worldspawnIndex];

			collapsed.MapObjects.Add(worldspawn);

			for (var i = 0; i < map.MapObjects.Count; i++)
			{
				if (i != worldspawnIndex)
				{
					var collapsedObject = map.MapObjects[i].Collapse();

					foreach (var co in collapsedObject)
					{
						if (co.Definition.ClassName == "worldspawn")
						{
							worldspawn.Renderables.AddRange(co.Renderables);
						}
						else
						{
							collapsed.MapObjects.Add(co);
						}
					}
				}
			}

			return collapsed;
		}
	}

	public class QuakeMap : Map
	{
		public List<Wad2> Wads;

		public QuakeMap() : base()
		{
		}
		public QuakeMap(QuakeMap map) : base(map)
		{
		}
		public QuakeMap(string filename, DefinitionDictionary definitions) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions)
		{
		}
		public QuakeMap(Stream stream, DefinitionDictionary definitions) : base(stream, definitions)
		{
		}
		public QuakeMap(string filename, DefinitionDictionary definitions, TextureCollection textures) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions, textures)
		{
		}
		public QuakeMap(Stream stream, DefinitionDictionary definitions, TextureCollection textures) :
			base(stream, definitions, textures)
		{
		}

		public sealed override void Parse(string raw)
		{
			var oldCwd = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(Path.GetDirectoryName(AbsolutePath ?? oldCwd));

			string stripped = raw.Replace("\r", String.Empty);
			stripped = stripped.Replace("\n", String.Empty);
			stripped = stripped.Replace("\t", String.Empty); // FIXME: Only strip leading tabs! Or just tabs not within a key or value?

			// Modern Quake sourceports allow for transparency in textures,
			// indicated by an open curly brace in the texture name. This
			// complicates the Regex necessary to split the map file. Any open
			// curly brace that's actually a delimiter will be followed by an
			// open parenthesis or one or more whitespace characters. Texture
			// names will instead have ordinary text characters after the brace.
			string curly = Regex.Escape(OpenDelimiter);

			// The whitespace here is outside the capture group, and will be
			// discarded, but it would get stripped later anyway.
			string whitespaceAfter = "(" + curly + ")\\s+";

			// These, however, are inside the capture group, and will be
			// preserved, but will end up in the same list item as the open
			// curly brace. A quick loop later on will clean that up.
			string parenthesisAfter = "(" + curly + "\\()";
			string quoteAfter = "(" + curly + "\")";

			string delimiters =
				whitespaceAfter + "|" +
				parenthesisAfter + "|" +
				quoteAfter + "|" +
				"(" + Regex.Escape(CloseDelimiter) + ")";

			List<string> split = Regex.Split(stripped, delimiters).ToList();
			split.RemoveAll(s => s.Trim() == "" || s.Trim() == "\"");

			// The regex patterns above include capture groups to retain some
			// delimiters in the output, but they end up in the same item in the
			// list resulting from Split. This ugly loop fixes that.
			var item = 0;
			while (item < split.Count)
			{
				if (split[item] == "{(")
				{
					split[item] = "{";
					split[item + 1] = "(" + split[item + 1];
					item++;
				}
				else if (split[item] == "{\"")
				{
					split[item] = "{";
					split[item + 1] = "\"" + split[item + 1];
					item++;
				}
				else
				{
					item++;
				}
			}

			var i = 0;
			while (i < split.Count)
			{
				var firstBraceIndex = split.IndexOf("{", i);

				var block = new QuakeBlock(split, firstBraceIndex, Definitions);
				MapObjects.Add(new QuakeMapObject(block, Definitions, TextureCollection));

				i = firstBraceIndex + block.RawLength;
			}

			foreach (var mo in MapObjects)
			{
				Aabb += mo.AABB;
			}

			Directory.SetCurrentDirectory(oldCwd);
		}

		public override string ToString()
		{
			return ToString(QuakeSideFormat.Valve220);
		}
		public string ToString(QuakeSideFormat format)
		{
			var sb = new StringBuilder();

			// First build the final worldspawn.
			int worldspawnIndex = MapObjects.FindIndex(o => o.Definition.ClassName == "worldspawn");
			MapObject worldspawn = MapObjects[worldspawnIndex];
			foreach (var mo in AllObjects)
			{
				// Only copy solids to worldspawn if that's all this entity
				// is set to save, otherwise let it get handled later.
				if (mo.Saveability == Saveability.Solids)
				{
					worldspawn.Renderables.AddRange(mo.Renderables);
				}
			}

			sb.AppendLine(new QuakeBlock(worldspawn).ToString(format));

			for (var i = 0; i < MapObjects.Count; i++)
			{
				if (i == worldspawnIndex)
				{
					continue;
				}

				var mo = MapObjects[i];

				// Avoid saving a null character to the output.
				if (mo.Saveability == Saveability.None)
				{
					continue;
				}

				sb.AppendLine(new QuakeBlock(mo).ToString(format));
			}

			return sb.ToString();
		}
	}
}
