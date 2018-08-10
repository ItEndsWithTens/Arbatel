using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Temblor.Formats.Quake
{
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
			Parse();
		}

		public sealed override void Parse()
		{
			string stripped = Raw.Replace("\r", "");
			stripped = stripped.Replace("\n", "");
			stripped = stripped.Replace("\t", ""); // FIXME: Only strip leading tabs! Or just tabs not within a key or value?

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
				MapObjects.Add(new QuakeMapObject(block, Definitions));

				i = firstBraceIndex + block.RawLength;
			}
		}
	}
}
