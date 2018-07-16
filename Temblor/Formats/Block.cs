using Eto.Gl;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class Block
	{
		public string OpenDelimiter = "{";
		public string CloseDelimiter = "}";
		public string KeyValDelimiter = "\"";

		public string BlockType = "";

		/// <summary>
		/// The starting index of the block, relative to the list it was parsed from.
		/// </summary>
		public int RawStartIndex;

		/// <summary>
		///  The number of elements this block occupies in the list it was parsed from.
		/// </summary>
		public int RawLength;

		public Dictionary<string, List<string>> KeyVals = new Dictionary<string, List<string>>();

		public List<Block> Children = new List<Block>();

		/// <summary>
		/// Whether this block contains information about outbound logic flow.
		/// </summary>
		/// <remarks>
		/// Blocks in Source engine VMFs, for example, don't indicate whether anything
		/// targets them, only whether they target anything else.
		/// </remarks>
		public bool HasConnectionsOut = false;

		public List<Renderable> Renderables = new List<Renderable>();

		public Block()
		{
		}

		public void Draw(Shader shader)
		{
			foreach (var child in Children)
			{
				child.Draw(shader);
			}

			foreach (var renderable in Renderables)
			{
				var model = Matrix4.CreateTranslation(renderable.Position);
				shader.SetMatrix4("model", ref model);
				renderable.Draw(shader);
			}
		}

		public void Init(List<GLSurface> surfaces)
		{
			foreach (var child in Children)
			{
				child.Init(surfaces);
			}

			foreach (var renderable in Renderables)
			{
				renderable.Init(surfaces);
			}
		}

		protected List<KeyValuePair<string, string>> ExtractKeyVals(string raw)
		{
			var keyVals = new List<KeyValuePair<string, string>>();

			// Using a pair of capturing parentheses in the regex pattern preserves
			// delimiters in the output.
			List<string> list = Regex.Split(raw, "(" + Regex.Escape(KeyValDelimiter) + ")").ToList();
			list.RemoveAll(s => s.Trim() == "");

			// With the raw input split on doublequotes, and those quotes preserved,
			// there's a predictable sequence of quote, key, quote, quote, value, quote,
			// and so on. Preserving the delimiters and doing things this way easily allows
			// for empty values, which are not uncommon.
			var i = 1;
			while (i < list.Count)
			{
				keyVals.Add(new KeyValuePair<string, string>(list[i], list[i + 3]));
				i += 6;
			}

			return keyVals;
		}

		protected int FindCloseBraceIndex(ref List<string> raw, int openBraceIndex)
		{
			int closeBraceIndex = openBraceIndex + 1;

			var braces = 1;
			for (var i = openBraceIndex + 1; i < raw.Count; ++i)
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
