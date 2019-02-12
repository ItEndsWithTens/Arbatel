using Arbatel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Formats
{
	public class Block
	{
		/// <summary>
		/// The DefinitionDictionary this block pulled its key/value defaults from.
		/// </summary>
		public DefinitionDictionary Definitions { get; } = new DefinitionDictionary();

		public string OpenDelimiter { get; set; } = "{";
		public string CloseDelimiter { get; set; } = "}";
		public string KeyValDelimiter { get; set; } = "\"";

		public string BlockType { get; set; } = "";

		/// <summary>
		/// The starting index of the block, relative to the list it was parsed from.
		/// </summary>
		public int RawStartIndex { get; set; }

		/// <summary>
		///  The number of elements this block occupies in the list it was parsed from.
		/// </summary>
		public int RawLength { get; set; }

		public Dictionary<string, Option> KeyVals { get; } = new Dictionary<string, Option>();

		public List<Block> Children { get; } = new List<Block>();

		/// <summary>
		/// Whether this block contains information about outbound logic flow.
		/// </summary>
		/// <remarks>
		/// Blocks in Source engine VMFs, for example, don't indicate whether anything
		/// targets them, only whether they target anything else.
		/// </remarks>
		public bool HasConnectionsOut { get; set; } = false;

		public Saveability Saveability { get; set; }

		public Block()
		{
		}

		protected List<KeyValuePair<string, string>> ExtractKeyVals(string raw)
		{
			var keyVals = new List<KeyValuePair<string, string>>();

			List<string> list = raw.SplitAndKeepDelimiters(KeyValDelimiter).ToList();

			// After splitting, the list may contain entries that are empty or
			// comprised only of whitespace. Empty entries are the result of the
			// input string having had newline characters removed. Whitespace is
			// from spaces in the original input that were inside or outside of
			// a value. Whitespace inside a value would be desirable if it were
			// always present in the case of a key/val that was present but not
			// defined; unfortunately it's common for undefined values to be
			// entirely empty, without even whitespace. Since there's no way to
			// reliably detect undefined values, blank list items are useless,
			// and can safely be stripped out.
			list.RemoveAll(s => String.IsNullOrEmpty(s.Trim()));

			// With no way to detect undefined values, the only reliable method
			// remaining is brute force: count delimiters one by one.
			int i = 0;
			while (i < list.Count)
			{
				// There are four double quote marks per key/value pair.
				int quotes = 0;

				string key = "";
				string value = "";

				while (quotes < 1)
				{
					if (list[i] == "\"")
					{
						quotes++;
					}

					i++;
				}

				key = list[i++];

				while (quotes < 3)
				{
					if (list[i] == "\"")
					{
						quotes++;
					}

					i++;
				}

				if (list[i] != "\"")
				{
					value = list[i++];
				}

				while (quotes < 4)
				{
					if (list[i] == "\"")
					{
						quotes++;
					}

					i++;
				}

				keyVals.Add(new KeyValuePair<string, string>(key, value));
			}

			return keyVals;
		}

		protected int FindCloseBraceIndex(List<string> raw, int openBraceIndex)
		{
			int closeBraceIndex = openBraceIndex + 1;

			int braces = 1;
			for (int i = openBraceIndex + 1; i < raw.Count; ++i)
			{
				if (raw[i] == OpenDelimiter)
				{
					++braces;
				}
				else if (raw[i] == CloseDelimiter)
				{
					--braces;
				}

				if (braces == 0)
				{
					closeBraceIndex = i;
					break;
				}
			}

			return closeBraceIndex;
		}
	}
}
