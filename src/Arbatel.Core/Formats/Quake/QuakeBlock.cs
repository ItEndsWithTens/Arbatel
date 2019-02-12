using Arbatel.Graphics;
using Arbatel.Utilities;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arbatel.Formats.Quake
{
	public class QuakeBlock : Block
	{
		public List<Solid> Solids { get; } = new List<Solid>();

		public QuakeBlock(List<string> rawList, int openBraceIndex, DefinitionDictionary definitions)
		{
			Definitions.Clear();
			foreach (KeyValuePair<string, Definition> pair in definitions)
			{
				Definitions.Add(pair.Key, pair.Value);
			}

			RawStartIndex = openBraceIndex;
			RawLength = (FindCloseBraceIndex(rawList, openBraceIndex) + 1) - RawStartIndex;

			Solids = new List<Solid>();

			List<string> rawBlock = rawList.GetRange(RawStartIndex, RawLength);

			Parse(rawBlock);
		}
		public QuakeBlock(MapObject mo)
		{
			QuakeMapObject qmo;
			if (mo is QuakeMapObject)
			{
				qmo = mo as QuakeMapObject;
			}
			else
			{
				throw new ArgumentException("Provided MapObject isn't actually a QuakeMapObject!");
			}

			Saveability = qmo.Saveability;

			foreach (MapObject child in qmo.Children)
			{
				Children.Add(new QuakeBlock(child as QuakeMapObject));
			}

			KeyVals.Clear();
			foreach (KeyValuePair<string, Option> pair in qmo.KeyVals)
			{
				KeyVals.Add(pair.Key, pair.Value);
			}

			if (qmo.Definition != null && qmo.Definition.ClassType == ClassType.Solid)
			{
				foreach (Renderable r in qmo.Renderables)
				{
					var solid = new Solid();

					foreach (Polygon p in r.Polygons)
					{
						var side = new QuakeSide
						{
							Plane = new Plane(r.Vertices[p.Indices[2]], r.Vertices[p.Indices[1]], r.Vertices[p.Indices[0]], Winding.Cw),

							TextureName = p.Texture.Name,
							TextureBasis = new List<Vector3>() { p.BasisS, p.BasisT },
							TextureOffset = p.Offset,
							TextureRotation = p.Rotation,
							TextureScale = p.Scale
						};

						solid.Sides.Add(side);
					}

					Solids.Add(solid);
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

			if ((Saveability & Saveability.Entity) == Saveability.Entity)
			{
				sb.AppendLine(OpenDelimiter);

				foreach (KeyValuePair<string, Option> keyVal in KeyVals)
				{
					sb.Append("\"");
					sb.Append(keyVal.Key);
					sb.Append("\" \"");
					sb.Append(keyVal.Value);
					sb.Append("\"");
					sb.Append(Environment.NewLine);
				}
			}

			if ((Saveability & Saveability.Solids) == Saveability.Solids)
			{
				foreach (Solid solid in Solids)
				{
					sb.AppendLine(OpenDelimiter);

					foreach (Side side in solid.Sides)
					{
						sb.AppendLine((side as QuakeSide).ToString(format));
					}

					sb.AppendLine(CloseDelimiter);
				}
			}

			if ((Saveability & Saveability.Children) == Saveability.Children)
			{
				foreach (Block child in Children)
				{
					sb.AppendLine(child.ToString());
				}
			}

			if ((Saveability & Saveability.Entity) == Saveability.Entity)
			{
				sb.Append(CloseDelimiter);
			}

			return sb.ToString();
		}

		private void Parse(List<string> rawBlock)
		{
			int i = 0;
			while (i < rawBlock.Count)
			{
				string item = rawBlock[i];

				if (item.Contains(KeyValDelimiter))
				{
					List<KeyValuePair<string, string>> rawKeyVals = ExtractKeyVals(item);

					string classname = rawKeyVals.Find(s => s.Key == "classname").Value;

					foreach (KeyValuePair<string, string> rawKeyVal in rawKeyVals)
					{
						Option newOption;
						if (Definitions.ContainsKey(classname) && Definitions[classname].KeyValsTemplate.ContainsKey(rawKeyVal.Key))
						{
							newOption = new Option(Definitions[classname].KeyValsTemplate[rawKeyVal.Key]);
						}
						else
						{
							newOption = new Option();
						}
						newOption.Value = rawKeyVal.Value;

						if (!KeyVals.ContainsKey(rawKeyVal.Key))
						{
							KeyVals.Add(rawKeyVal.Key, newOption);
						}
						else
						{
							KeyVals[rawKeyVal.Key] = newOption;
						}
					}

					++i;
				}
				// An open delimiter that's the first item in the raw block is just the
				// start of the top level block; any others indicate children.
				else if (item == OpenDelimiter && i != 0)
				{
					int childOpenBraceIndex = i + 1;

					var childBlock = new QuakeBlock(rawBlock, childOpenBraceIndex, Definitions);
					Children.Add(childBlock);

					i = childBlock.RawStartIndex + childBlock.RawLength;
				}
				else if (item.StartsWith("(", StringComparison.OrdinalIgnoreCase))
				{
					string solid = item;
					int j = i + 1;
					while (rawBlock[j] != CloseDelimiter)
					{
						solid += rawBlock[j];
						j++;
					}

					Solids.Add(new Solid(ExtractSides(solid)));

					i += j;
				}
				else
				{
					i++;
				}
			}
		}

		private static List<Side> ExtractSides(string raw)
		{
			List<string> split = raw.SplitAndKeepDelimiters("(", ")").ToList();
			split.RemoveAll(s => String.IsNullOrEmpty(s.Trim()));

			var sides = new List<Side>();
			for (int i = 0; i < split.Count; i += 10)
			{
				sides.Add(new QuakeSide(String.Join(" ", split.GetRange(i, 10))));
			}

			return sides;
		}
	}
}
