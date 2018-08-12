using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats.Quake
{
	public class QuakeBlock : Block
	{
		public List<QuakeSide> Sides = new List<QuakeSide>();

		public QuakeBlock(List<string> rawList, int openBraceIndex)
		{
			RawStartIndex = openBraceIndex;
			RawLength = (FindCloseBraceIndex(rawList, openBraceIndex) + 1) - RawStartIndex;

			var rawBlock = rawList.GetRange(RawStartIndex, RawLength);

			Parse(rawBlock);
		}
		public QuakeBlock(MapObject mo) : this(mo as QuakeMapObject)
		{
		}
		public QuakeBlock(QuakeMapObject qmo)
		{
			foreach (var child in qmo.Children)
			{
				Children.Add(new QuakeBlock(child));
			}

			KeyVals = qmo.KeyVals;

			if (qmo.Definition != null && qmo.Definition.ClassType == ClassType.Solid)
			{
				foreach (var r in qmo.Renderables)
				{
					foreach (var p in r.Polygons)
					{
						var side = new QuakeSide();

						side.Plane = new Plane(r.Vertices[p.Indices[2]], r.Vertices[p.Indices[1]], r.Vertices[p.Indices[0]], Winding.Cw);

						side.TextureName = p.TextureName;
						side.TextureBasis = new List<Vector3>() { p.BasisS, p.BasisT };
						side.TextureOffset = p.Offset;
						side.TextureRotation = p.Rotation;
						side.TextureScale = p.Scale;

						Sides.Add(side);
					}
				}
			}
		}

		public override string ToString()
		{
			return ToString(QuakeSideFormat.Valve220);
		}
		public string ToString(QuakeSideFormat format)
		{
			var sb = new StringBuilder();

			sb.AppendLine(OpenDelimiter);

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
					sb.Append("\"");
					sb.Append(Environment.NewLine);
				}
			}

			foreach (var side in Sides)
			{
				sb.AppendLine(side.ToString(format));
			}

			foreach (var child in Children)
			{
				sb.Append(child.ToString());
			}

			sb.Append(CloseDelimiter);

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

					var childBlock = new QuakeBlock(rawBlock, childOpenBraceIndex);
					Children.Add(childBlock);

					i = childBlock.RawStartIndex + childBlock.RawLength;
				}
				else if (item.StartsWith("("))
				{
					foreach (var side in ExtractSides(item))
					{
						Sides.Add(new QuakeSide(side));
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
