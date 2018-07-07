using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class QuakeMap : Map
	{
		public QuakeMap(Stream stream) : base(stream)
		{
			Parse();
		}

		public sealed override void Parse()
		{
			string delimiters = "(" + Regex.Escape(OpenDelimiter) + "|" + Regex.Escape(CloseDelimiter) + ")";

			string stripped = Raw.Replace("\r", "");
			stripped = stripped.Replace("\n", "");
			stripped = stripped.Replace("\t", ""); // FIXME: Only strip leading tabs! Or just tabs not within a key or value?

			List<string> split = Regex.Split(stripped, delimiters).ToList();
			split.RemoveAll(s => s.Trim() == "" || s.Trim() == "\"");

			var i = 0;
			while (i < split.Count)
			{
				var firstBraceIndex = split.IndexOf("{", i);

				// Works, but shouldn't be necessary. I want to be able to pass into the QuakeBlock constructor
				// the entire block, from open brace through close brace, since that's conceptually more sensible. Not
				// make users (e.g. me in the future) remember to strip the first brace of a map or block off of it. Need to
				// work on the logic inside the block parsing code, I think.
				//var firstBraceIndex = split.IndexOf("{", i) + 1;

				var newBlock = new QuakeBlock(ref split, firstBraceIndex);
				Blocks.Add(newBlock);

				i = firstBraceIndex + newBlock.RawLength;
			}
		}
	}
}
