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
		public static QuakeMap Collapse(this QuakeMap map)
		{
			var result = new QuakeMap();

			MapObject worldspawn = map.MapObjects.Find(o => o.Definition.ClassName == "worldspawn");
			var newWorldspawn = new QuakeMapObject(worldspawn as QuakeMapObject)
			{
				Renderables = new List<Renderable>()
			};

			foreach (var mo in map.MapObjects)
			{
				if (mo.Definition.ClassName == "worldspawn")
				{
					foreach (var renderable in mo.Renderables)
					{
						if (renderable is QuakeBrush b)
						{
							newWorldspawn.Renderables.Add(b);
						}
					}
				}
				else if (mo.UserData is QuakeMap m)
				{
					var collapsed = m.Collapse();

					newWorldspawn.Renderables.AddRange(collapsed.MapObjects[0].Renderables);

					if (collapsed.MapObjects.Count > 1)
					{
						result.MapObjects.AddRange(collapsed.MapObjects.GetRange(1, collapsed.MapObjects.Count - 1));
					}
				}
				else
				{
					result.MapObjects.Add(mo);
				}
			}

			result.MapObjects.Reverse();
			result.MapObjects.Add(newWorldspawn);
			result.MapObjects.Reverse();

			return result;
		}
	}

	public class QuakeMap : Map
	{
		public List<Wad2> Wads;

		public QuakeMap() : base()
		{
		}
		public QuakeMap(string filename, DefinitionCollection definitions) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions)
		{
		}
		public QuakeMap(Stream stream, DefinitionCollection definitions) : base(stream, definitions)
		{
		}
		public QuakeMap(string filename, DefinitionCollection definitions, TextureCollection textures) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions, textures)
		{
		}
		public QuakeMap(Stream stream, DefinitionCollection definitions, TextureCollection textures) :
			base(stream, definitions, textures)
		{
		}

		public sealed override void Parse(string raw)
		{
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

				var block = new QuakeBlock(split, firstBraceIndex);
				MapObjects.Add(new QuakeMapObject(block, Definitions, TextureCollection));

				i = firstBraceIndex + block.RawLength;
			}
		}

		public override string ToString()
		{
			return ToString(QuakeSideFormat.Valve220);
		}
		public string ToString(QuakeSideFormat format)
		{
			var sb = new StringBuilder();

			foreach (var mo in MapObjects)
			{
				sb.AppendLine(new QuakeBlock(mo).ToString(format));
			}

			return sb.ToString();
		}
	}
}
