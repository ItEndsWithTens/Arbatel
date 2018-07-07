using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	class QuakeBlock : Block
	{
		private List<string> _sides = new List<string>();

		public QuakeBlock(ref List<string> rawList, int openBraceIndex)
		{
			RawStartIndex = openBraceIndex;
			RawLength = (FindCloseBraceIndex(ref rawList, openBraceIndex) + 1) - RawStartIndex;

			var rawBlock = rawList.GetRange(RawStartIndex, RawLength);

			Parse(rawBlock);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("{\n");

			foreach (var keyVal in KeyVals)
			{
				// There can occasionally be duplicate keys, so to ensure none
				// of them are forgotten, loop on the value.
				foreach (var value in keyVal.Value)
				{
					sb.Append("\"");
					sb.Append(keyVal.Key);
					sb.Append("\" \"");
					sb.Append(value);
					sb.Append("\"\n");
				}
			}

			foreach (var side in _sides)
			{
				sb.Append(side + "\n");
			}

			foreach (var child in Children)
			{
				sb.Append(child.ToString());
			}

			sb.Append("}\n");

			return sb.ToString();
		}

		private void Parse(List<string> rawBlock)
		{
			var i = 0;
			while (i < rawBlock.Count)
			{
				var item = rawBlock[i];

				if (item.Contains(KeyValDelimiter))
				{
					var rawKeyVals = ExtractKeyVals(item);

					foreach (var rawKeyVal in rawKeyVals)
					{
						if (!KeyVals.ContainsKey(rawKeyVal.Key))
						{
							var list = new List<string>() { rawKeyVal.Value };
							KeyVals.Add(rawKeyVal.Key, list);
						}
						else
						{
							KeyVals[rawKeyVal.Key].Add(rawKeyVal.Value);
						}
					}

					++i;
				}
				// An open delimiter that's the first item in the raw block is just the
				// start of the top level block; any others indicate children.
				else if (item == OpenDelimiter && i != 0)
				{
					var childOpenBraceIndex = i + 1;

					var childBlock = new QuakeBlock(ref rawBlock, childOpenBraceIndex);
					Children.Add(childBlock);

					i = childBlock.RawStartIndex + childBlock.RawLength;
				}
				else if (item.StartsWith("("))
				{
					foreach(var side in ExtractSides(item))
					{
						_sides.Add(side);
					}

					++i;
				}
				else
				{
					++i;
				}
			}
		}

		private List<string> ExtractSides(string raw)
		{
			var sides = new List<string>();

			var delimiters = "(\\(|\\))";

			List<string> split = Regex.Split(raw, delimiters).Select(s => s.Trim()).ToList();
			split.RemoveAll(s => s.Trim() == "");

			for (var i = 0; i < split.Count; i += 10)
			{
				sides.Add(String.Join(" ", split.GetRange(i, 10)));
			}

			return sides;
		}
	}
}
